using System.Threading.Tasks;
using Ganpa.Strategies;

namespace Ganpa.ClientFactory
{
    public interface IGanpaClientFactory
    {
        // TODO custom strategies는 settings 안으로 옮겨주자.

        IGanpa CreateClient(GanpaSettings settings, bool synchronousInitialization = false,
            params IStrategy[] strategies);

        Task<IGanpa> CreateClientAsync(GanpaSettings settings, bool synchronousInitialization = false,
            params IStrategy[] strategies);
    }
}