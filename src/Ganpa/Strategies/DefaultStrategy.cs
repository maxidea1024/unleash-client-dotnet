namespace Ganpa.Strategies
{
    using System.Collections.Generic;
    using Ganpa.Internal;

    public class DefaultStrategy : IStrategy
    {
        private const string STRATEGY_NAME = "default";

        public string Name => STRATEGY_NAME;

        public bool IsEnabled(Dictionary<string, string> parameters, GanpaContext? context = null)
        {
            return true;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, GanpaContext context,
            IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}