using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Ganpa.Events;
using Ganpa.Internal;
using Ganpa.Logging;
using Ganpa.Metrics;
using Ganpa.Serialization;

namespace Ganpa.Communication
{
    internal class GanpaApiClient : IGanpaApiClient
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(GanpaApiClient));

        private readonly HttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly GanpaApiClientRequestHeaders _clientRequestHeaders;
        private readonly EventCallbackConfig _eventConfig;
        private readonly string _projectId;
        private int _featureRequestsToSkip = 0;
        private int _featureRequestsSkipped = 0;
        private int _metricsRequestsToSkip = 0;
        private int _metricsRequestsSkipped = 0;

        private readonly int[] _backoffResponses =
            new int[]
            {
                429,
                500,
                502,
                503,
                504
            };

        private readonly int[] _configurationErrorResponses =
            new int[]
            {
                401,
                403,
                404,
            };

        public GanpaApiClient(
            HttpClient httpClient,
            IJsonSerializer jsonSerializer,
            GanpaApiClientRequestHeaders clientRequestHeaders,
            EventCallbackConfig eventConfig,
            string projectId = null)
        {
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
            _clientRequestHeaders = clientRequestHeaders;
            _eventConfig = eventConfig;
            _projectId = projectId;
        }

        public async Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken,
            bool throwOnFail = false)
        {
            if (_featureRequestsToSkip > _featureRequestsSkipped)
            {
                _featureRequestsSkipped++;
                return new FetchTogglesResult
                {
                    HasChanged = false,
                    Etag = null,
                };
            }

            _featureRequestsSkipped = 0;

            var resourceUri = "client/features";

            // TODO deprecated?
            if (!string.IsNullOrWhiteSpace(_projectId))
            {
                resourceUri += "?project=" + this._projectId;
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, resourceUri))
            {
                SetRequestHeaders(request, _clientRequestHeaders);

                if (EntityTagHeaderValue.TryParse(etag, out var etagHeaderValue))
                {
                    request.Headers.IfNoneMatch.Add(etagHeaderValue);
                }

                using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotModified)
                    {
                        return await HandleErrorResponse(response, resourceUri, throwOnFail);
                    }

                    return await HandleSuccessResponse(response, etag);
                }
            }
        }

        private async Task<FetchTogglesResult> HandleErrorResponse(HttpResponseMessage response, string resourceUri,
            bool shouldThrow = false)
        {
            if (_backoffResponses.Contains((int)response.StatusCode))
            {
                Backoff(response);
            }

            if (_configurationErrorResponses.Contains((int)response.StatusCode))
            {
                ConfigurationError(response, resourceUri);
            }

            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Logger.Trace(
                () => $"GANPA: Error {response.StatusCode} from server in '{nameof(FetchToggles)}': " + error);
            _eventConfig?.RaiseError(new ErrorEvent()
                { ErrorType = ErrorType.Client, StatusCode = response.StatusCode, Resource = resourceUri });

            if (shouldThrow)
            {
                throw new GanpaException($"Ganpa: {response.StatusCode} from server in '{nameof(FetchToggles)}': " +
                                           error);
            }

            return new FetchTogglesResult
            {
                HasChanged = false,
                Etag = null,
            };
        }

        private void Backoff(HttpResponseMessage response)
        {
            _featureRequestsToSkip = Math.Min(10, _featureRequestsToSkip + 1);
            Logger.Warn(() =>
                $"UNLEASH: Backing off due to {response.StatusCode} from server in '{nameof(FetchToggles)}'.");
        }

        private void ConfigurationError(HttpResponseMessage response, string requestUri)
        {
            _featureRequestsToSkip = 10;

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    Logger.Error(() =>
                        $"UNLEASH: Error when fetching toggles, {requestUri} responded NOT_FOUND (404) which means your API url most likely needs correction.'.");
                    break;
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    Logger.Error(() =>
                        $"UNLEASH: Error when fetching toggles, {requestUri} responded FORBIDDEN (403) which means your API token is not valid.");
                    break;
                default:
                    Logger.Error(() =>
                        $"UNLEASH: Configuration error due to {response.StatusCode} from server in '{nameof(FetchToggles)}'.");
                    break;
            }
        }

        private async Task<FetchTogglesResult> HandleSuccessResponse(HttpResponseMessage response, string etag)
        {
            _featureRequestsToSkip = Math.Max(0, _featureRequestsToSkip - 1);

            var newEtag = response.Headers.ETag?.Tag;
            if (newEtag == etag || response.StatusCode == HttpStatusCode.NotModified)
            {
                return new FetchTogglesResult
                {
                    HasChanged = false,
                    Etag = newEtag,
                    ToggleCollection = null,
                };
            }

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var toggleCollection = _jsonSerializer.Deserialize<ToggleCollection>(stream);

            if (toggleCollection == null)
            {
                return new FetchTogglesResult
                {
                    HasChanged = false
                };
            }

            // Success
            return new FetchTogglesResult
            {
                HasChanged = true,
                Etag = newEtag,
                ToggleCollection = toggleCollection
            };
        }

        public async Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken)
        {
            const string requestUri = "client/register";

            var memoryStream = new MemoryStream();
            _jsonSerializer.Serialize(memoryStream, registration);

            const int bufferSize = 1024 * 4;

            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                request.Content = new StreamContent(memoryStream, bufferSize);
                request.Content.Headers.AddContentTypeJson();

                SetRequestHeaders(request, _clientRequestHeaders);

                using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }

                    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Logger.Trace(() =>
                        $"UNLEASH: Error {response.StatusCode} from request '{requestUri}' in '{nameof(GanpaApiClient)}': " +
                        error);
                    _eventConfig?.RaiseError(new ErrorEvent()
                        { Resource = requestUri, ErrorType = ErrorType.Client, StatusCode = response.StatusCode });

                    return false;
                }
            }
        }

        public async Task<bool> SendMetrics(ThreadSafeMetricsBucket metrics, CancellationToken cancellationToken)
        {
            if (_metricsRequestsToSkip > _metricsRequestsSkipped)
            {
                _metricsRequestsSkipped++;
                return false;
            }

            _metricsRequestsSkipped = 0;

            const string requestUri = "client/metrics";

            var memoryStream = new MemoryStream();

            using (metrics.StopCollectingMetrics(out var bucket))
            {
                _jsonSerializer.Serialize(memoryStream, new ClientMetrics
                {
                    AppName = _clientRequestHeaders.AppName,
                    InstanceId = _clientRequestHeaders.InstanceTag,
                    Bucket = bucket
                });
            }

            const int bufferSize = 1024 * 4;

            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                request.Content = new StreamContent(memoryStream, bufferSize);
                request.Content.Headers.AddContentTypeJson();

                SetRequestHeaders(request, _clientRequestHeaders);

                using (var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotModified)
                    {
                        HandleMetricsSuccessResponse(response);
                        return true;
                    }

                    await HandleMetricsErrorResponse(response, requestUri);
                    return false;
                }
            }
        }

        private async Task HandleMetricsErrorResponse(HttpResponseMessage response, string requestUri)
        {
            if (_backoffResponses.Contains((int)response.StatusCode))
            {
                _metricsRequestsToSkip = Math.Min(10, _metricsRequestsToSkip + 1);
            }

            if (_configurationErrorResponses.Contains((int)response.StatusCode))
            {
                _metricsRequestsToSkip = 10;
            }

            var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Logger.Trace(() =>
                $"UNLEASH: Error {response.StatusCode} from request '{requestUri}' in '{nameof(GanpaApiClient)}': " +
                error);
            _eventConfig?.RaiseError(new ErrorEvent()
                { Resource = requestUri, ErrorType = ErrorType.Client, StatusCode = response.StatusCode });
        }

        private void HandleMetricsSuccessResponse(HttpResponseMessage response)
        {
            _metricsRequestsToSkip = Math.Max(0, _metricsRequestsToSkip - 1);
        }

        private static void SetRequestHeaders(HttpRequestMessage requestMessage, GanpaApiClientRequestHeaders headers)
        {
            // TODO rename
            const string appNameHeader = "UNLEASH-APPNAME";
            const string userAgentHeader = "User-Agent";
            const string instanceIdHeader = "UNLEASH-INSTANCEID";

            const string supportedSpecVersionHeader = "Ganpa-Client-Spec";

            requestMessage.Headers.TryAddWithoutValidation(appNameHeader, headers.AppName);
            requestMessage.Headers.TryAddWithoutValidation(userAgentHeader, headers.AppName);
            requestMessage.Headers.TryAddWithoutValidation(instanceIdHeader, headers.InstanceTag);
            requestMessage.Headers.TryAddWithoutValidation(supportedSpecVersionHeader, headers.SupportedSpecVersion);

            SetCustomHeaders(requestMessage, headers.CustomHttpHeaders);
            SetCustomHeaders(requestMessage, headers.CustomHttpHeaderProvider?.CustomHeaders);
        }

        private static void SetCustomHeaders(HttpRequestMessage requestMessage, Dictionary<string, string> headers)
        {
            if (headers == null)
            {
                return;
            }

            if (headers.Count == 0)
            {
                return;
            }

            foreach (var header in headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }
}