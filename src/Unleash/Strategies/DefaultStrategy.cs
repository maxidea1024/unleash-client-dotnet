namespace Unleash.Strategies
{
    using System.Collections.Generic;
    using Unleash.Internal;

    public class DefaultStrategy : IStrategy
    {
        private const string STRATEGY_NAME = "default";

        public string Name => STRATEGY_NAME;

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext? context = null)
        {
            return true;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context,
            IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}