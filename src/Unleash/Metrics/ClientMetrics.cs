namespace Unleash.Metrics
{
    internal class ClientMetrics
    {
        public string AppName { get; set; }

        public string InstanceId { get; set; }

        public Yggdrasil.MetricsBucket Bucket { get; set; }

        public string PlatformName => MetricsMetadata.GetPlatformName();

        public string PlatformVersion => MetricsMetadata.GetPlatformVersion();

        public string YggdrasilVersion => "0.14.0";

        public string SpecVersion => UnleashServices.supportedSpecVersion;
    }
}