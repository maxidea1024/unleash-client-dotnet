using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ganpa.Communication;
using Ganpa.Logging;
using Ganpa.Metrics;

namespace Ganpa.Scheduling
{
    internal class ClientRegistrationBackgroundTask : IGanpaScheduledTask
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientRegistrationBackgroundTask));

        private readonly IGanpaApiClient _apiClient;
        private readonly GanpaSettings _settings;
        private readonly List<string> _strategies;

        public string Name => "register-client-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }

        public ClientRegistrationBackgroundTask(
            IGanpaApiClient apiClient,
            GanpaSettings settings,
            List<string> strategies)
        {
            _apiClient = apiClient;
            _settings = settings;
            _strategies = strategies;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_settings.SendMetricsInterval == null)
            {
                return;
            }

            var clientRegistration = new ClientRegistration
            {
                AppName = _settings.AppName,
                InstanceId = _settings.InstanceTag,
                Interval = (long)_settings.SendMetricsInterval.Value.TotalMilliseconds,
                SdkVersion = _settings.SdkVersion,
                Started = DateTimeOffset.UtcNow,
                Strategies = _strategies
            };

            var result = await _apiClient.RegisterClient(clientRegistration, cancellationToken).ConfigureAwait(false);
            if (!result)
            {
                // Already logged.
            }
        }
    }
}