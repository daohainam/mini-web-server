using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.OutputCaching
{
    public class OutputCacheStreamInfo
    {
        public required IHttpContent Content { get; set; }
        public required HttpResponseCodes StatusCode { get; set; }
        public required HttpHeader Headers { get; set; }
    }
}
