using Ganpa.Internal;

namespace Ganpa.Communication
{
    internal class FetchTogglesResult
    {
        public ToggleCollection ToggleCollection { get; set; }

        public bool HasChanged { get; set; }

        public string Etag { get; set; }
    }
}