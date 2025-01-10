using System.Threading;
using System.Threading.Tasks;
using Ganpa.Strategies;

namespace Ganpa.ClientFactory
{
    /// <inheritdoc />
    public class GanpaClientFactory : IGanpaClientFactory
    {
        private static readonly TaskFactory TaskFactory =
            new TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        /// <summary>
        /// Initializes a new instance of Ganpa client. 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="synchronousInitialization">If true, fetch and cache toggles before returning. If false, allow the ganpa client schedule an initial poll of features in the background</param>
        /// <param name="strategies">Custom strategies, added in addition to builtIn strategies.</param>
        public IGanpa CreateClient(GanpaSettings settings, bool synchronousInitialization = false,
            params IStrategy[] strategies)
        {
            if (!synchronousInitialization)
            {
                return new DefaultGanpa(settings, strategies);
            }

            settings.ScheduleFeatureToggleFetchImmediately = false;
            settings.ThrowOnInitialFetchFail = true;

            var ganpa = new DefaultGanpa(settings, strategies);
            
            TaskFactory
                .StartNew(() => ganpa.Services.FetchFeatureTogglesTask.ExecuteAsync(CancellationToken.None))
                .Unwrap()
                .GetAwaiter()
                .GetResult();

            return ganpa;
        }

        /// <summary>
        /// Initializes a new instance of Ganpa client. 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="synchronousInitialization">If true, fetch and cache toggles before returning. If false, allow the ganpa client schedule an initial poll of features in the background</param>
        /// <param name="strategies">Custom strategies, added in addition to builtIn strategies.</param>
        public async Task<IGanpa> CreateClientAsync(GanpaSettings settings, bool synchronousInitialization = false,
            params IStrategy[] strategies)
        {
            if (!synchronousInitialization)
            {
                return new DefaultGanpa(settings, strategies);
            }

            settings.ScheduleFeatureToggleFetchImmediately = false;
            settings.ThrowOnInitialFetchFail = true;
            
            var ganpa = new DefaultGanpa(settings, strategies);
            
            await ganpa.Services.FetchFeatureTogglesTask.ExecuteAsync(CancellationToken.None)
                .ConfigureAwait(false);
            
            return ganpa;
        }
    }
}