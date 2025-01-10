namespace Ganpa.Internal
{
    internal static class GanpaSettingsValidator
    {
        public static void Validate(GanpaSettings settings)
        {
            if (string.IsNullOrEmpty(settings.GanpaApiUri))
            {
                throw new GanpaException("You are required to specify an uri to an ganpa service");
            }

            if (string.IsNullOrEmpty(settings.AppName))
            {
                throw new GanpaException("You are required to specify an appName");
            }

            if (string.IsNullOrEmpty(settings.InstanceTag))
            {
                throw new GanpaException("You are required to specify an instance id");
            }

            if (settings.JsonSerializer == null)
            {
                throw new GanpaException("You are required to specify an json serializer");
            }

            settings.JsonSerializer =
                DynamicJsonLibraryChooser.CheckIfJsonSerializerCanBeInitialized(settings.JsonSerializer);
        }
    }
}