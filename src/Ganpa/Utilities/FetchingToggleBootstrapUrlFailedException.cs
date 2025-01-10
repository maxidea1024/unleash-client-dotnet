using System.Net;

namespace Ganpa.Utilities
{
    public class FetchingToggleBootstrapUrlFailedException : GanpaException
    {
        public HttpStatusCode StatusCode { get; set; }

        public FetchingToggleBootstrapUrlFailedException(string errorMessage, HttpStatusCode statusCode) : base(
            errorMessage)
        {
            StatusCode = statusCode;
        }

        public FetchingToggleBootstrapUrlFailedException(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }
    }
}