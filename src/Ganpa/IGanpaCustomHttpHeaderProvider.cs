using System.Collections.Generic;

namespace Ganpa
{
    public interface IGanpaCustomHttpHeaderProvider
    {
        Dictionary<string, string> CustomHeaders { get; }
    }
}