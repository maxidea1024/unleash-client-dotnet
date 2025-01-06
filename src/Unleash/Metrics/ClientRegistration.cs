using System;
using System.Collections.Generic;

namespace Unleash.Metrics
{
    internal class ClientRegistration
    {
        public string AppName { get; set; }

        public string InstanceId { get; set; }

        public string SdkVersion { get; set; }

        public List<string> Strategies { get; set; }

        public DateTimeOffset Started { get; set; }

        public long Interval { get; set; }

        public string PlatformName => MetricsMetadata.GetPlatformName();

        public string PlatformVersion => MetricsMetadata.GetPlatformVersion();

        public string YggdrasilVersion => "0.14.0";

        public string SpecVersion => UnleashServices.supportedSpecVersion;
    }
}