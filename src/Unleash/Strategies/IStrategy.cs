using System;
using Yggdrasil;

namespace Unleash.Strategies
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a strategy for enabling a feature.
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// Gets the strategy name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Calculates if the strategy is enabled for a given context
        /// </summary>
        bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context);
    }

    internal class CustomStrategyAdapter : Yggdrasil.IStrategy
    {
        private IStrategy _strategy;

        public CustomStrategyAdapter(IStrategy strategy)
        {
            _strategy = strategy;
        }

        public string Name => _strategy.Name;

        public bool IsEnabled(Dictionary<string, string> parameters, Context context)
        {
            var currentTime = context.CurrentTime ?? DateTimeOffset.UtcNow;

            var unleashContext = new UnleashContext.Builder()
                                                    .AppName(context.AppName)
                                                    .CurrentTime(currentTime)
                                                    .UserId(context.UserId)
                                                    .SessionId(context.SessionId)
                                                    .RemoteAddress(context.RemoteAddress)
                                                    .Build();
            unleashContext.Properties = context.Properties;

            return _strategy.IsEnabled(parameters, unleashContext);
        }
    }
}
