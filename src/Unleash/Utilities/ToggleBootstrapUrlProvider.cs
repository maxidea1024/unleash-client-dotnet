using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Internal;
using Unleash.Logging;

namespace Unleash.Utilities
{
    public class ToggleBootstrapUrlProvider : IToggleBootstrapProvider
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ToggleBootstrapUrlProvider));

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpClient _client;
        private readonly UnleashSettings _settings;
        private readonly string _path;
        private readonly bool _throwOnFail;
        private readonly Dictionary<string, string> _customHeaders;

        public ToggleBootstrapUrlProvider(string path, HttpClient client, UnleashSettings settings, bool throwOnFail = false, Dictionary<string, string> customHeaders = null)
        {
            _path = path;
            _client = client;
            _settings = settings;
            _throwOnFail = throwOnFail;
            _customHeaders = customHeaders;
        }

        public string Read()
        {
            return Task.Run(() => FetchFile()).GetAwaiter().GetResult();
        }

        private async Task<string> FetchFile()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, _path))
            {
                if (_customHeaders != null)
                {
                    foreach (var keyValuePair in _customHeaders)
                    {
                        request.Headers.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                using (var response = await _client.SendAsync(request, _cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        Logger.Trace(() => $"GANPA: Error {response.StatusCode} from server in 'ToggleBootstrapUrlProvider.{nameof(FetchFile)}': " + error);

                        if (_throwOnFail)
                        {
                            throw new FetchingToggleBootstrapUrlFailedException("Failed to fetch feature toggles", response.StatusCode);
                        }

                        return null;
                    }

                    try
                    {
                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.Trace(() => $"GANPA: Exception in 'ToggleBootstrapUrlProvider.{nameof(FetchFile)}' during reading and deserializing ToggleCollection from stream: " + ex.Message);

                        if (_throwOnFail)
                        {
                            throw new UnleashException("Exception during reading and deserializing ToggleCollection from stream", ex);
                        }

                        return null;
                    }
                }
            }
        }
    }
}
