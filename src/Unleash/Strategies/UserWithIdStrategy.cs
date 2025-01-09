namespace Unleash.Strategies
{
    using System;
    using System.Collections.Generic;
    using Unleash.Internal;

    /// <inheritdoc />
    public class UserWithIdStrategy : IStrategy
    {
        private const string USER_IDS_CONST = "userIds";

        /// <inheritdoc />
        public string Name => "userWithId";

        /// <inheritdoc />
        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext? context = null)
        {
            var userId = context?.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            if (!parameters.TryGetValue(USER_IDS_CONST, out var userIds))
            {
                return false;
            }

            const string commaDelimiter = ",";
            const string space = " ";

            var idsLocal = string.Concat(commaDelimiter, userIds.Replace(space, string.Empty), commaDelimiter);
            var userLocal = string.Concat(commaDelimiter, userId.Replace(space, string.Empty), commaDelimiter);

            return idsLocal.IndexOf(userLocal, StringComparison.Ordinal) > -1;
        }

        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context,
            IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}