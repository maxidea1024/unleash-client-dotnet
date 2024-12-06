using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Logging;
using Unleash.Metrics;

namespace Unleash.Scheduling
{
    internal class ClientRegistrationBackgroundTask : IUnleashScheduledTask
    {
        public string Name => "register-client-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }

        // private static readonly ILog Logger = LogProvider.GetLogger(typeof(ClientRegistrationBackgroundTask));
        private readonly IUnleashApiClient _apiClient;
        private readonly UnleashSettings _settings;
        private readonly List<string> _strategies;

        public ClientRegistrationBackgroundTask(
            IUnleashApiClient apiClient,
            UnleashSettings settings,
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
                // Already logged..
            }
        }
    }
}
