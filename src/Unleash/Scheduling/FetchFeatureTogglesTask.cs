using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unleash.Communication;
using Unleash.Internal;
using Unleash.Logging;
using Unleash.Events;
using System.Net.Http;
using Yggdrasil;

namespace Unleash.Scheduling
{
    internal class FetchFeatureTogglesTask : IUnleashScheduledTask
    {
        public string Name => "fetch-feature-toggles-task";
        public TimeSpan Interval { get; set; }
        public bool ExecuteDuringStartup { get; set; }

        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));
        private readonly string _toggleFile;
        private readonly string _etagFile;
        private readonly IFileSystem _fileSystem;
        private readonly EventCallbackConfig _eventConfig;
        private readonly IUnleashApiClient _apiClient;
        private readonly YggdrasilEngine _engine;
        private readonly bool _throwOnInitialLoadFail;
        private bool _ready = false;

        // In-memory reference of toggles/etags
        internal string Etag { get; set; }

        public FetchFeatureTogglesTask(
            YggdrasilEngine engine,
            IUnleashApiClient apiClient,
            IFileSystem fileSystem,
            EventCallbackConfig eventConfig,
            string toggleFile,
            string etagFile,
            bool throwOnInitialLoadFail)
        {
            _engine = engine;
            _apiClient = apiClient;
            _fileSystem = fileSystem;
            _eventConfig = eventConfig;
            _toggleFile = toggleFile;
            _etagFile = etagFile;
            _throwOnInitialLoadFail = throwOnInitialLoadFail;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            FetchTogglesResult result;
            try
            {
                result = await _apiClient.FetchToggles(Etag, cancellationToken, !_ready && this.throwOnInitialLoadFail).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                Logger.Warn(() => $"GANPA: Unhandled exception when fetching toggles.", ex);
                eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.Client, Error = ex });
                throw new UnleashException("Exception while fetching from API", ex);
            }

            _ready = true;

            if (!result.HasChanged)
            {
                return;
            }

            if (string.IsNullOrEmpty(result.Etag))
			{
				return;
			}

            if (result.Etag == Etag)
			{
				return;
			}

            if (!string.IsNullOrEmpty(result.State))
            {
                try
                {
                    engine.TakeState(result.State);
                }
                catch (Exception ex)
                {
                    Logger.Warn(() => $"GANPA: Exception when updating toggle collection.", ex);
                    _eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.TogglesUpdate, Error = ex });
                    throw new UnleashException("Exception while updating toggle collection", ex);
                }
            }

            // now that the toggle collection has been updated, raise the toggles updated event if configured
            _eventConfig?.RaiseTogglesUpdated(new TogglesUpdatedEvent { UpdatedOn = DateTime.UtcNow });

            try
            {
                fileSystem.WriteAllText(_toggleFile, result.State);
            }
            catch (IOException ex)
            {
                Logger.Warn(() => $"GANPA: Exception when writing to toggle file '{toggleFile}'.", ex);
                _eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.TogglesBackup, Error = ex });
            }

            Etag = result.Etag;

            try
            {
                fileSystem.WriteAllText(_etagFile, Etag);
            }
            catch (IOException ex)
            {
                Logger.Warn(() => $"GANPA: Exception when writing to ETag file '{etagFile}'.", ex);
                _eventConfig?.RaiseError(new ErrorEvent() { ErrorType = ErrorType.TogglesBackup, Error = ex });
            }
        }
    }
}
