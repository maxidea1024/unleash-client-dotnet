using System;
using System.Collections.Generic;
using System.Threading;

namespace Ganpa.Scheduling
{
    /// <inheritdoc />
    /// <summary>
    /// Task manager for scheduling tasks on a background thread. 
    /// </summary>
    public interface IGanpaScheduledTaskManager : IDisposable
    {
        /// <summary>
        /// Configures a set of tasks to execute in the background.
        /// </summary>
        /// <param name="tasks">Tasks to be executed</param>
        /// <param name="cancellationToken">Cancellation token which will be passed during shutdown (Dispose).</param>
        void Configure(IEnumerable<IGanpaScheduledTask> tasks, CancellationToken cancellationToken);
    }
}