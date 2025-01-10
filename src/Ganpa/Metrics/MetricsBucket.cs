using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ganpa.Metrics
{
    /// <inheritdoc />
    /// <summary>
    /// Provides synchronization that supports multiple registration counters and single 'writer' (transfer to server)
    ///
    /// While in write mode, no registrations will occur. i.e: no lock for rest of system.
    /// </summary>
    internal class ThreadSafeMetricsBucket : IDisposable
    {
        private long _missedRegistrations;
        public long MissedRegistrations => _missedRegistrations;

        private readonly MetricsBucket _metricsBucket;

        private readonly ReaderWriterLockSlim _lock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        public ThreadSafeMetricsBucket(MetricsBucket metricsBucket = null)
        {
            _metricsBucket = metricsBucket ?? new MetricsBucket();

            _metricsBucket.Toggles = new ConcurrentDictionary<string, ToggleCount>();
            _metricsBucket.Start = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Registers a new toggle count given a read-lock can be acquired.
        /// </summary>
        /// <param name="toggleName">The name of the toggle.</param>
        /// <param name="active">True or False</param>
        public void RegisterCount(string toggleName, bool active)
        {
            WithToggleCount(toggleName, toggle => toggle.Register(active));
        }

        public void RegisterCount(string toggleName, string variantName)
        {
            WithToggleCount(toggleName, toggle => toggle.Register(variantName));
        }

        private void WithToggleCount(string toggleName, Action<ToggleCount> action)
        {
            if (_lock.TryEnterReadLock(2))
            {
                try
                {
                    var toggle = _metricsBucket.Toggles.GetOrAdd(toggleName, x => new ToggleCount());
                    action(toggle);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            else
            {
                // Ignore
                Interlocked.Increment(ref _missedRegistrations);
            }
        }

        /// <summary>
        /// Use withing using-statement. New registrations will not be added.
        /// </summary>
        public IDisposable StopCollectingMetrics(out MetricsBucket bucket)
        {
            _lock.EnterWriteLock();

            bucket = _metricsBucket;
            bucket.Stop = DateTimeOffset.UtcNow;

            return this;
        }

        /// <inheritdoc />
        /// <summary>
        /// Resets the counters to 0.
        /// </summary>
        void IDisposable.Dispose()
        {
            ResetCounters();
            _lock.ExitWriteLock();
        }

        private void ResetCounters()
        {
            _metricsBucket.Start = DateTimeOffset.UtcNow;

            foreach (var item in _metricsBucket.Toggles)
            {
                item.Value.Reset();
            }
        }
    }

    internal class MetricsBucket
    {
        public ConcurrentDictionary<string, ToggleCount> Toggles { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset Stop { get; set; }
    }
}