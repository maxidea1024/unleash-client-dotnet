using System.Collections.Generic;

namespace Ganpa
{
    internal class DefaultGanpaContextProvider : IGanpaContextProvider
    {
        public GanpaContext Context { get; }

        public DefaultGanpaContextProvider(GanpaContext? context = null)
        {
            Context = context ?? new GanpaContext
            {
                Properties = new Dictionary<string, string>(0),
            };
        }
    }
}