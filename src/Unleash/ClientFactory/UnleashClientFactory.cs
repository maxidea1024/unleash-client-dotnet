using System.Threading;
using System.Threading.Tasks;
using Unleash.Strategies;

namespace Unleash.ClientFactory
{
    /// <inheritdoc />
    public class UnleashClientFactory : IUnleashClientFactory
    {
        private static readonly TaskFactory TaskFactory =
            new TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        /// <summary>
        /// Initializes a new instance of Unleash client. 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="synchronousInitialization">If true, fetch and cache toggles before returning. If false, allow the unleash client schedule an initial poll of features in the background</param>
        /// <param name="strategies">Custom strategies, added in addition to builtIn strategies.</param>
        public IUnleash CreateClient(UnleashSettings settings, bool synchronousInitialization = false,
            params IStrategy[] strategies)
        {
            if (!synchronousInitialization)
            {
                return new DefaultUnleash(settings, strategies);
            }

            settings.ScheduleFeatureToggleFetchImmediately = false;
            settings.ThrowOnInitialFetchFail = true;
            var unleash = new DefaultUnleash(settings, strategies);
            TaskFactory
                .StartNew(() => unleash.Services.FetchFeatureTogglesTask.ExecuteAsync(CancellationToken.None))
                .Unwrap()
                .GetAwaiter()
                .GetResult();

            return unleash;
        }


        /// <summary>
        /// Initializes a new instance of Unleash client. 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="synchronousInitialization">If true, fetch and cache toggles before returning. If false, allow the unleash client schedule an initial poll of features in the background</param>
        /// <param name="strategies">Custom strategies, added in addition to builtIn strategies.</param>
        public async Task<IUnleash> CreateClientAsync(UnleashSettings settings, bool synchronousInitialization = false,
            params IStrategy[] strategies)
        {
            if (!synchronousInitialization)
            {
                return new DefaultUnleash(settings, strategies);
            }

            settings.ScheduleFeatureToggleFetchImmediately = false;
            settings.ThrowOnInitialFetchFail = true;
            var unleash = new DefaultUnleash(settings, strategies);
            await unleash.Services.FetchFeatureTogglesTask.ExecuteAsync(CancellationToken.None)
                .ConfigureAwait(false);
            return unleash;
        }
    }
}