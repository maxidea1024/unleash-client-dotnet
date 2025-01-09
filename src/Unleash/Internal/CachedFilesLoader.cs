using System.IO;
using Unleash.Events;
using Unleash.Logging;
using Unleash.Scheduling;
using Unleash.Serialization;

namespace Unleash.Internal
{
    internal class CachedFilesLoader
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(FetchFeatureTogglesTask));

        private readonly IJsonSerializer _jsonSerializer;
        private readonly IFileSystem _fileSystem;
        private readonly IToggleBootstrapProvider _toggleBootstrapProvider;
        private readonly EventCallbackConfig _eventConfig;
        private readonly string _toggleFile;
        private readonly string _etagFile;
        private readonly bool _bootstrapOverride;

        public CachedFilesLoader(
            IJsonSerializer jsonSerializer,
            IFileSystem fileSystem,
            IToggleBootstrapProvider toggleBootstrapProvider,
            EventCallbackConfig eventConfig,
            string toggleFile,
            string etagFile,
            bool bootstrapOverride = true)
        {
            _jsonSerializer = jsonSerializer;
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
                    Logger.Error(() => $"UNLEASH: Unhandled exception when writing to ETag file '{_etagFile}'.", ex);
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
                    Logger.Error(() => $"UNLEASH: Unhandled exception when reading from ETag file '{_etagFile}'.", ex);
                    _eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }

            // Toggles
            if (!_fileSystem.FileExists(_toggleFile))
            {
                try
                {
                    _fileSystem.WriteAllText(_toggleFile, string.Empty);
                    result.InitialToggleCollection = null;
                }
                catch (IOException ex)
                {
                    Logger.Error(() => $"UNLEASH: Unhandled exception when writing to toggle file '{_toggleFile}'.",
                        ex);
                    _eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }
            else
            {
                try
                {
                    using (var fileStream = _fileSystem.FileOpenRead(_toggleFile))
                    {
                        result.InitialToggleCollection = _jsonSerializer.Deserialize<ToggleCollection>(fileStream);
                    }
                }
                catch (IOException ex)
                {
                    Logger.Error(() => $"UNLEASH: Unhandled exception when reading from toggle file '{_toggleFile}'.",
                        ex);
                    _eventConfig?.RaiseError(new ErrorEvent() { Error = ex, ErrorType = ErrorType.FileCache });
                }
            }

            if (result.InitialToggleCollection == null)
            {
                result.InitialETag = string.Empty;
            }

            if ((result.InitialToggleCollection != null && result.InitialToggleCollection.Features?.Count != 0 &&
                 !_bootstrapOverride) || _toggleBootstrapProvider == null)
            {
                return result;
            }

            var bootstrapCollection = _toggleBootstrapProvider.Read();
            if (bootstrapCollection != null && bootstrapCollection.Features?.Count > 0)
            {
                result.InitialToggleCollection = bootstrapCollection;
            }

            return result;
        }

        internal class CachedFilesResult
        {
            public string InitialETag { get; set; }

            public ToggleCollection InitialToggleCollection { get; set; }
        }
    }
}