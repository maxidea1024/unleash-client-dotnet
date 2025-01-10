using System;
using System.Linq;
using Ganpa.Internal;

namespace Ganpa.Strategies.Constraints
{
    public class StringConstraintOperator : IConstraintOperator
    {
        public bool Evaluate(Constraint constraint, GanpaContext context)
        {
            if (constraint.Values == null || !constraint.Values.Any())
            {
                return false;
            }

            var contextValue = context.GetByName(constraint.ContextName);
            if (string.IsNullOrWhiteSpace(contextValue))
            {
                return false;
            }

            // TODO invert 처리는 외부에서 하는게 좋지 않을까?
            var result = constraint.Values.Any(val =>
                Eval(constraint.Operator, val, contextValue, constraint.CaseInsensitive));
            return !constraint.Inverted ? result : !result;
        }

        private bool Eval(string @operator, string value, string contextValue, bool caseInsensitive)
        {
            var comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (@operator)
            {
                case Operator.STR_CONTAINS:
                    return contextValue.IndexOf(value, comparison) > -1;
                case Operator.STR_ENDS_WITH:
                    return contextValue.EndsWith(value, comparison);
                case Operator.STR_STARTS_WITH:
                    return contextValue.StartsWith(value, comparison);
                default:
                    return false;
            }
        }
    }
}