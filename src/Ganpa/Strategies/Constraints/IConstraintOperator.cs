using Ganpa.Internal;

namespace Ganpa.Strategies.Constraints
{
    public interface IConstraintOperator
    {
        bool Evaluate(Constraint constraint, GanpaContext context);
    }
}