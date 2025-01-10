using Ganpa.Internal;

namespace Ganpa.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    /// <inheritdoc />
    public class ApplicationHostnameStrategy : IStrategy
    {
        private const string HOST_NAMES_PARAM = "hostNames";
        private const string NAME_CONST = "applicationHostname";

        // TODO 한번만 가져오면 될듯.
        private readonly string _hostname = Environment.GetEnvironmentVariable("hostname") ?? Dns.GetHostName();

        /// <inheritdoc />
        public string Name => NAME_CONST;

        /// <inheritdoc />
        public bool IsEnabled(Dictionary<string, string> parameters, GanpaContext? context = null)
        {
            if (!parameters.TryGetValue(HOST_NAMES_PARAM, out var hostnames))
            {
                return false;
            }

            if (string.IsNullOrEmpty(hostnames))
            {
                return false;
            }

            return hostnames
                .ToLowerInvariant()
                .Split(',')
                .Select(x => x.Trim())
                .Contains(_hostname.ToLowerInvariant());
        }

        public bool IsEnabled(Dictionary<string, string> parameters, GanpaContext context,
            IEnumerable<Constraint> constraints)
        {
            return StrategyUtils.IsEnabled(this, parameters, context, constraints);
        }
    }
}