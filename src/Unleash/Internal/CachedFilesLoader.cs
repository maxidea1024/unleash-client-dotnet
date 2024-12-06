using System.IO;
using Unleash.Events;
using Unleash.Logging;
using Unleash.Scheduling;

namespace Unleash.Internal
{
    internal class CachedFilesLoader
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));

        private readonly IFileSystem _fileSystem;
        private readonly IToggleBootstrapProvider _toggleBootstrapProvider;
        private readonly EventCallbackConfig _eventConfig;
        private readonly string _toggleFile;
        private readonly string _etagFile;
        private readonly bool _bootstrapOverride;

        public CachedFilesLoader(
            IFileSystem fileSystem,
            IToggleBootstrapProvider toggleBootstrapProvider,
            EventCallbackConfig eventConfig,
            string toggleFile,
            string etagFile,
            bool bootstrapOverride = true)
        {
            _fileSystem = fileSystem;
            _toggleBootstrapProvider = toggleBootstrapProvider;
            _eventConfig = eventConfig;
            _toggleFile = toggleFile;
            _etagFile = etagFile;
            _bootstrapOverride = bootstrapOverride;
        }

        public CachedFilesResult EnsureExistsAndLoad()
        {
            var result = new CachedFilesResult();

            if (!_fileSystem.FileExists(_etagFile))
            {
                // Ensure files exists.
                try
                {
                    _fileSystem.WriteAllText(_etagFile, string.Empty);
                    result.InitialETag = string.Empty;
                }
                catch (IOException ex)
                {
                    Logger.Error(() => $"GANPA: Unhandled exception when writing to ETag file '{_etagFile}'.", ex);
                    _eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }
            else
            {
                try
                {
                    result.InitialETag = _fileSystem.ReadAllText(_etagFile);
                }
                catch (IOException ex)
                {
                    Logger.Error(() => $"GANPA: Unhandled exception when reading from ETag file '{_etagFile}'.", ex);
                    _eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }

            // Toggles
            if (!_fileSystem.FileExists(_toggleFile))
            {
                try
                {
                    _fileSystem.WriteAllText(_toggleFile, string.Empty);
                    result.InitialState = string.Empty;
                }
                catch (IOException ex)
                {
                    Logger.Error(() => $"GANPA: Unhandled exception when writing to toggle file '{_toggleFile}'.", ex);
                    _eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }
            else
            {
                try
                {
                    result.InitialState = _fileSystem.ReadAllText(_toggleFile);
                }
                catch (IOException ex)
                {
                    Logger.Error(() => $"GANPA: Unhandled exception when reading from toggle file '{_toggleFile}'.", ex);
                    _eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }

            if (string.IsNullOrEmpty(result.InitialState))
            {
                result.InitialETag = string.Empty;
            }

            if ((string.IsNullOrEmpty(result.InitialState) || _bootstrapOverride) && _toggleBootstrapProvider != null)
            {
                var bootstrapState = _toggleBootstrapProvider.Read();
                if (!string.IsNullOrEmpty(bootstrapState))
                {
                    result.InitialState = bootstrapState;
                }
            }

            return result;
        }

        internal class CachedFilesResult
        {
            public string InitialETag { get; set; }
            public string InitialState { get; set; }
        }
    }
}
