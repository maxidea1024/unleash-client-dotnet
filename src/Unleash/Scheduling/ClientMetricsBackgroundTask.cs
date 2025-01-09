using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Logging;
using Unleash.Metrics;

namespace Unleash.Scheduling
{
    internal class ClientMetricsBackgroundTask : IUnleashScheduledTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientMetricsBackgroundTask));

        private readonly IUnleashApiClient _apiClient;
        private readonly UnleashSettings _settings;
        private readonly ThreadSafeMetricsBucket _metricsBucket;

        public string Name => "report-metrics-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }

        public ClientMetricsBackgroundTask(
            IUnleashApiClient apiClient,
            UnleashSettings settings,
            ThreadSafeMetricsBucket metricsBucket)
        {
            _apiClient = apiClient;
            _settings = settings;
            _metricsBucket = metricsBucket;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_settings.SendMetricsInterval == null)
            {
                return;
            }

            var result = await _apiClient.SendMetrics(_metricsBucket, cancellationToken).ConfigureAwait(false);

            // Ignore return value
            if (!result)
            {
                // Logged elsewhere.
            }
        }
    }
}