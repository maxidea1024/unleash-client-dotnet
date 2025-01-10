using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ganpa.Communication;
using Ganpa.Internal;
using Ganpa.Metrics;
using Ganpa.Scheduling;
using Ganpa.Strategies;

namespace Ganpa
{
    internal class GanpaServices : IDisposable
    {
        private const string SUPPORTED_SPEC_VERSION = "4.5.1";

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IGanpaScheduledTaskManager _scheduledTaskManager;

        internal CancellationToken CancellationToken { get; }
        internal IGanpaContextProvider ContextProvider { get; }
        internal ThreadSafeToggleCollection ToggleCollection { get; }
        internal bool IsMetricsDisabled { get; }
        internal ThreadSafeMetricsBucket MetricsBucket { get; }
        internal FetchFeatureTogglesTask FetchFeatureTogglesTask { get; }

        public GanpaServices(GanpaSettings settings, EventCallbackConfig eventConfig,
            Dictionary<string, IStrategy> strategyMap)
        {
            if (settings.FileSystem == null)
            {
                settings.FileSystem = new FileSystem(settings.Encoding);
            }

            var backupFile = settings.GetFeatureToggleFilePath();
            var etagBackupFile = settings.GetFeatureToggleETagFilePath();

            // Cancellation
            CancellationToken = _cancellationTokenSource.Token;
            ContextProvider = settings.GanpaContextProvider;

            var loader = new CachedFilesLoader(settings.JsonSerializer, settings.FileSystem,
                settings.ToggleBootstrapProvider, eventConfig, backupFile, etagBackupFile, settings.BootstrapOverride);
            var cachedFilesResult = loader.EnsureExistsAndLoad();

            ToggleCollection = new ThreadSafeToggleCollection
            {
                Instance = cachedFilesResult.InitialToggleCollection ?? new ToggleCollection()
            };

            MetricsBucket = new ThreadSafeMetricsBucket();

            IGanpaApiClient apiClient;
            if (settings.GanpaApiClient == null)
            {
                var uri = settings.GanpaApiUri ?? throw new ArgumentNullException("settings.GanpaApiUri");
                if (!uri.AbsolutePath.EndsWith("/"))
                {
                    uri = new Uri($"{uri.AbsoluteUri}/");
                }

                var httpClient = settings.HttpClientFactory.Create(uri);
                apiClient = new GanpaApiClient(httpClient, settings.JsonSerializer,
                    new GanpaApiClientRequestHeaders()
                    {
                        AppName = settings.AppName,
                        InstanceTag = settings.InstanceTag,
                        // TODO Add ApiToken here
                        CustomHttpHeaders = settings.CustomHttpHeaders,
                        CustomHttpHeaderProvider = settings.GanpaCustomHttpHeaderProvider,
                        SupportedSpecVersion = SUPPORTED_SPEC_VERSION
                    }, eventConfig, settings.ProjectId);
            }
            else
            {
                // Mocked backend: fill instance collection 
                apiClient = settings.GanpaApiClient;
            }

            _scheduledTaskManager = settings.ScheduledTaskManager;

            IsMetricsDisabled = settings.SendMetricsInterval == null;

            var fetchFeatureTogglesTask = new FetchFeatureTogglesTask(
                apiClient,
                ToggleCollection,
                settings.JsonSerializer,
                settings.FileSystem,
                eventConfig,
                backupFile,
                etagBackupFile,
                settings.ThrowOnInitialFetchFail)
            {
                ExecuteDuringStartup = settings.ScheduleFeatureToggleFetchImmediately,
                Interval = settings.FetchTogglesInterval,
                Etag = cachedFilesResult.InitialETag
            };
            FetchFeatureTogglesTask = fetchFeatureTogglesTask;

            var scheduledTasks = new List<IGanpaScheduledTask>()
            {
                fetchFeatureTogglesTask
            };

            if (settings.SendMetricsInterval != null)
            {
                var clientRegistrationBackgroundTask = new ClientRegistrationBackgroundTask(
                    apiClient,
                    settings,
                    strategyMap.Select(pair => pair.Key).ToList())
                {
                    Interval = TimeSpan.Zero,
                    ExecuteDuringStartup = true
                };

                scheduledTasks.Add(clientRegistrationBackgroundTask);

                var clientMetricsBackgroundTask = new ClientMetricsBackgroundTask(
                    apiClient,
                    settings,
                    MetricsBucket)
                {
                    Interval = settings.SendMetricsInterval.Value
                };

                scheduledTasks.Add(clientMetricsBackgroundTask);
            }

            _scheduledTaskManager.Configure(scheduledTasks, CancellationToken);
        }

        public void Dispose()
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            _scheduledTaskManager?.Dispose();
            ToggleCollection?.Dispose();
        }
    }
}