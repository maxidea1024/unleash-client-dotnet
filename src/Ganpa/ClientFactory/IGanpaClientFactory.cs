using System.Threading.Tasks;
using Ganpa.Strategies;

namespace Ganpa.ClientFactory
{
    public interface IGanpaClientFactory
    {
        IGanpa CreateClient(GanpaSettings settings, bool synchronousInitialization = false,
            params IStrategy[] strategies);

        Task<IGanpa> CreateClientAsync(GanpaSettings settings, bool synchronousInitialization = false,
            params IStrategy[] strategies);
    }
}