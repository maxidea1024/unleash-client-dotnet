using System.Collections.Generic;

namespace Ganpa
{
    internal class DefaultCustomHttpHeaderProvider : IGanpaCustomHttpHeaderProvider
    {
        // TODO readonly collection
        public Dictionary<string, string> CustomHeaders => new Dictionary<string, string>();
    }
}