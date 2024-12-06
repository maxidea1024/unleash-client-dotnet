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

        public string Read()
        {
            return _settings.FileSystem.ReadAllText(_filePath);
        }
    }
}
