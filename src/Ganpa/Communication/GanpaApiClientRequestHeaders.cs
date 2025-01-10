using System.Collections.Generic;

namespace Ganpa.Communication
{
    internal class GanpaApiClientRequestHeaders
    {
        public string AppName { get; set; }

        public string InstanceTag { get; set; }

        // TODO apiToken은 프로퍼티 형태로 위치시키는게 좋을듯.

        public Dictionary<string, string> CustomHttpHeaders { get; set; }

        public IGanpaCustomHttpHeaderProvider CustomHttpHeaderProvider { get; set; }

        public string SupportedSpecVersion { get; internal set; }
    }
}