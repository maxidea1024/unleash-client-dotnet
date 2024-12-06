using System;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Logging;
using Yggdrasil;

namespace Unleash.Scheduling
{
    internal class ClientMetricsBackgroundTask : IUnleashScheduledTask
    {
        public string Name => "report-metrics-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }

        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientMetricsBackgroundTask));

        private readonly YggdrasilEngine _engine;
        private readonly IUnleashApiClient _apiClient;
        private readonly UnleashSettings _settings;

        public ClientMetricsBackgroundTask(
            YggdrasilEngine engine,
            IUnleashApiClient apiClient,
            UnleashSettings settings)
        {
            _engine = engine;
            _apiClient = apiClient;
            _settings = settings;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_settings.SendMetricsInterval == null)
            {
                return;
            }

            var result = await _apiClient.SendMetrics(_engine.GetMetrics(), cancellationToken).ConfigureAwait(false);

            // Ignore return value
            if (!result)
            {
                // Logged elsewhere.
            }
        }
    }
}
