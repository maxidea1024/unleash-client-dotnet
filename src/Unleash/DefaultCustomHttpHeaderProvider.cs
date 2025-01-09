using System.Collections.Generic;

namespace Unleash
{
    internal class DefaultCustomHttpHeaderProvider : IUnleashCustomHttpHeaderProvider
    {
        // TODO readonly collection
        public Dictionary<string, string> CustomHeaders => new Dictionary<string, string>();
    }
}