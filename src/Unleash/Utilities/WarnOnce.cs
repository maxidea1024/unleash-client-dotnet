using System;
using System.Collections.Generic;
using Unleash.Logging;

namespace Unleash.Utilities
{
    internal class WarnOnce
    {
        private readonly ILog _logger;
        private readonly HashSet<string> _seen = new HashSet<string>();

        public WarnOnce(ILog logger)
        {
            _logger = logger;
        }

        public void Warn(string key, string message)
        {
            if (_seen.Contains(key))
            {
                return;
            }

            _seen.Add(key);
            _logger.Warn(() => message);
        }
    }
}
