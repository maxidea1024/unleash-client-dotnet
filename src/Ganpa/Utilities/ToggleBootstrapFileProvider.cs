using Ganpa.Internal;

namespace Ganpa.Utilities
{
    public class ToggleBootstrapFileProvider : IToggleBootstrapProvider
    {
        private readonly string _filePath;
        private readonly GanpaSettings _settings;

        internal ToggleBootstrapFileProvider(string filePath, GanpaSettings settings)
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