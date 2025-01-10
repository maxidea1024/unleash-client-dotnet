using System.Collections.Generic;
using System.Linq;
using Ganpa.Internal;
using Ganpa.Strategies.Constraints;

namespace Ganpa.Strategies
{
    public class ConstraintUtils
    {
        private static readonly Dictionary<string, IConstraintOperator> Operators =
            new Dictionary<string, IConstraintOperator>()
            {
                { Operator.STR_CONTAINS, new StringConstraintOperator() },
                { Operator.STR_ENDS_WITH, new StringConstraintOperator() },
                { Operator.STR_STARTS_WITH, new StringConstraintOperator() },
                { Operator.NUM_EQ, new NumberConstraintOperator() },
                { Operator.NUM_GT, new NumberConstraintOperator() },
                { Operator.NUM_GTE, new NumberConstraintOperator() },
                { Operator.NUM_LT, new NumberConstraintOperator() },
                { Operator.NUM_LTE, new NumberConstraintOperator() },
                { Operator.DATE_AFTER, new DateConstraintOperator() },
                { Operator.DATE_BEFORE, new DateConstraintOperator() },
                { Operator.SEMVER_EQ, new SemverConstraintOperator() },
                { Operator.SEMVER_GT, new SemverConstraintOperator() },
                { Operator.SEMVER_LT, new SemverConstraintOperator() },
            };

        public static bool Validate(IEnumerable<Constraint> constraints, GanpaContext context)
        {
            // No need to check count - all returns true if no elements
            return constraints == null || constraints.All(c => ValidateConstraint(c, context));
        }

        private static bool ValidateConstraint(Constraint constraint, GanpaContext context)
        {
            if (constraint == null)
            {
                return false;
            }

            var contextValue = context.GetByName(constraint.ContextName);
            if (constraint.Operator == null)
            {
                return false;
            }

            if (Operators.TryGetValue(constraint.Operator, out var @operator))
            {
                return @operator.Evaluate(constraint, context);
            }
            else
            {
                var isIn = contextValue != null && constraint.Values.Contains(contextValue.Trim());

                switch (constraint.Operator)
                {
                    case Operator.IN:
                        return constraint.Inverted ? !isIn : isIn;
                    case Operator.NOT_IN:
                        return constraint.Inverted ? isIn : !isIn;
                }
            }

            return false;
        }
    }
}