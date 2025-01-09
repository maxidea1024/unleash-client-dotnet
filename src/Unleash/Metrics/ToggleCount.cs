using System.Collections.Concurrent;
using System.Threading;

namespace Unleash.Metrics
{
    internal class ToggleCount
    {
        private long _yes;
        private long _no;
        private ConcurrentDictionary<string, long> _variants = new ConcurrentDictionary<string, long>();

        public long Yes => _yes;
        public long No => _no;
        public ConcurrentDictionary<string, long> Variants => _variants;

        public void Register(bool active)
        {
            if (active)
            {
                Interlocked.Increment(ref _yes);
            }
            else
            {
                Interlocked.Increment(ref _no);
            }
        }

        public void Register(string variantName)
        {
            _variants.AddOrUpdate(variantName, 1, (k, v) => v + 1);
        }

        /// <summary>
        /// Resets the counters to 0
        /// </summary>
        public void Reset()
        {
            _yes = 0;
            _no = 0;
            _variants = new ConcurrentDictionary<string, long>();
        }
    }
}