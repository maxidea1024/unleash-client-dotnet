using NuGet.Versioning;
using Unleash.Internal;
using Unleash.Logging;

namespace Unleash.Strategies.Constraints
{
    public class SemverConstraintOperator : IConstraintOperator
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(SemverConstraintOperator));

        public bool Evaluate(Constraint constraint, UnleashContext context)
        {
            var contextValue = context.GetByName(constraint.ContextName);
            if (!SemanticVersion.TryParse(contextValue, out var contextSemver))
            {
                Logger.Info(() => $"Couldn't parse version {contextValue} from context");
                return false;
            }

            if (string.IsNullOrWhiteSpace(constraint.Value)) return false;
            if (!SemanticVersion.TryParse(constraint.Value, out var constraintSemver))
            {
                return false;
            }

            var result = Eval(constraint.Operator, contextSemver, constraintSemver);
            return !constraint.Inverted ? result : !result;
        }

        private static bool Eval(string @operator, SemanticVersion contextSemver, SemanticVersion constraintSemver)
        {
            switch (@operator)
            {
                case Operator.SEMVER_GT:
                    return contextSemver > constraintSemver;
                case Operator.SEMVER_EQ:
                    return contextSemver == constraintSemver;
                case Operator.SEMVER_LT:
                    return contextSemver < constraintSemver;
                default:
                    return false;
            }
        }
    }
}