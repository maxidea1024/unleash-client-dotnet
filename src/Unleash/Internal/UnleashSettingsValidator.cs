namespace Unleash.Internal
{
    internal static class UnleashSettingsValidator
    {
        public static void Validate(UnleashSettings settings)
        {
            if (settings.UnleashApi == null)
            {
                throw new UnleashException("You are required to specify an uri to an unleash service");
            }

            if (settings.AppName == null)
            {
                throw new UnleashException("You are required to specify an appName");
            }

            if (settings.InstanceTag == null)
            {
                throw new UnleashException("You are required to specify an instance id");
            }

            if (settings.JsonSerializer == null)
            {
                throw new UnleashException("You are required to specify an json serializer");
            }

            settings.JsonSerializer =
                DynamicJsonLibraryChooser.CheckIfJsonSerializerCanBeInitialized(settings.JsonSerializer);
        }
    }
}