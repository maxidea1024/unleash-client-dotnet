using System.Threading;
using System.Threading.Tasks;
using Ganpa.Metrics;

namespace Ganpa.Communication
{
    internal interface IGanpaApiClient
    {
        Task<FetchTogglesResult> FetchToggles(string etag, CancellationToken cancellationToken,
            bool throwOnFail = false);

        Task<bool> RegisterClient(ClientRegistration registration, CancellationToken cancellationToken);

        Task<bool> SendMetrics(ThreadSafeMetricsBucket metrics, CancellationToken cancellationToken);
    }
}