using Unleash.Internal;

namespace Unleash.Utilities
{
    public class ToggleBootstrapFileProvider : IToggleBootstrapProvider
    {
        private readonly string _filePath;
        private readonly UnleashSettings _settings;

        internal ToggleBootstrapFileProvider(string filePath, UnleashSettings settings)
        {
            _filePath = filePath;
            _settings = settings;
        }

        public ToggleCollection Read()
        {
            using (var togglesStream = _settings.FileSystem.FileOpenRead(_filePath))
            {
                return _settings.JsonSerializer.Deserialize<ToggleCollection>(togglesStream);
            }
        }
    }
}