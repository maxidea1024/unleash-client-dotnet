using Unleash.Internal;

namespace Unleash.Strategies.Constraints
{
    public interface IConstraintOperator
    {
        bool Evaluate(Constraint constraint, UnleashContext context);
    }
}