using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ganpa.Communication;
using Ganpa.Logging;
using Ganpa.Metrics;

namespace Ganpa.Scheduling
{
    internal class ClientMetricsBackgroundTask : IGanpaScheduledTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientMetricsBackgroundTask));

        private readonly IGanpaApiClient _apiClient;
        private readonly GanpaSettings _settings;
        private readonly ThreadSafeMetricsBucket _metricsBucket;

        public string Name => "report-metrics-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }

        public ClientMetricsBackgroundTask(
            IGanpaApiClient apiClient,
            GanpaSettings settings,
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