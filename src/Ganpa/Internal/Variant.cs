using Ganpa.Variants;

namespace Ganpa.Internal
{
    public class Variant
    {
        public static readonly Variant DISABLED_VARIANT = new Variant("disabled", null, false, false);

        public string Name { get; }

        public Payload? Payload { get; }

        public bool IsEnabled { get; }

        public bool FeatureEnabled { get; internal set; }

        public Variant(string name, Payload? payload, bool enabled, bool featureEnabled)
        {
            Name = name;
            Payload = payload;
            IsEnabled = enabled;

            FeatureEnabled = featureEnabled;
        }
    }
}