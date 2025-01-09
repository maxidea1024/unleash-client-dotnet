using System;
using System.Globalization;
using Unleash.Internal;

namespace Unleash.Strategies.Constraints
{
    public class NumberConstraintOperator : IConstraintOperator
    {
        public bool Evaluate(Constraint constraint, UnleashContext context)
        {
            var contextValueString = context.GetByName(constraint.ContextName);
            if (string.IsNullOrWhiteSpace(contextValueString))
            {
                return false;
            }

            if (!double.TryParse(contextValueString, NumberStyles.Number, CultureInfo.InvariantCulture,
                    out var contextNumber))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(constraint.Value) || !double.TryParse(constraint.Value, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out var constraintNumber))
            {
                return false;
            }

            var result = Eval(constraint.Operator, constraintNumber, contextNumber);
            return !constraint.Inverted ? result : !result;
        }

        private static bool Eval(string @operator, double constraintNumber, double contextNumber)
        {
            const double tolerance = 1e6;

            switch (@operator)
            {
                case Operator.NUM_EQ:
                    return Math.Abs(contextNumber - constraintNumber) < tolerance;
                case Operator.NUM_GT:
                    return contextNumber > constraintNumber;
                case Operator.NUM_GTE:
                    return contextNumber >= constraintNumber;
                case Operator.NUM_LT:
                    return contextNumber < constraintNumber;
                case Operator.NUM_LTE:
                    return contextNumber <= constraintNumber;
                default:
                    return false;
            }
        }
    }
}