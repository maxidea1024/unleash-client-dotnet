using System;
using System.Threading;

namespace Unleash.Internal
{
    internal interface IObjectLock<T> : IDisposable
    {
        /// <summary>
        /// Gets or sets the instance of type T in a thread safe manner
        /// </summary>
        T Instance { get; set; }
    }

    /// <summary>
    /// Provides synchronization control that supports multiple readers and single writer over a given object T.
    /// </summary>
    internal class ReaderWriterLockSlimOf<T> : IObjectLock<T>
    {
        private readonly ReaderWriterLockSlim _lock;

        public ReaderWriterLockSlimOf(LockRecursionPolicy recursionPolicy = LockRecursionPolicy.NoRecursion)
        {
            _lock = new ReaderWriterLockSlim(recursionPolicy);
        }

        private T _instance;
        public T Instance
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _instance;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    _instance = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        public int CurrentReadCount => _lock.CurrentReadCount;

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}
