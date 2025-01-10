using System;
using System.Net.Http;

namespace Ganpa
{
    /// <summary>
    /// Factory for creating HttpClient used to communicate with Ganpa Server api.
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Called a single time during application initialization.
        /// </summary>
        HttpClient Create(Uri ganpaApiUri);
    }
}