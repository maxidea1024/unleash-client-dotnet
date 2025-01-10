namespace Ganpa.Strategies
{
    using System.Collections.Generic;
    using Ganpa.Internal;

    public class UnknownStrategy : IStrategy
    {
        public string Name => "unknown";

        public bool IsEnabled(Dictionary<string, string> parameters, GanpaContext? context = null)
        {
            return false;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, GanpaContext context,
            IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}