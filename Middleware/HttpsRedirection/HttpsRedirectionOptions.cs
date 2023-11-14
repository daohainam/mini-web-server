using MiniWebServer.Abstractions;

namespace MiniWebServer.HttpsRedirection
{
    public class HttpsRedirectionOptions
    {
        public int HttpsPort { get; set; } = 443;
        public HttpResponseCodes HttpResponseCode { get; set; } = HttpResponseCodes.TemporaryRedirect;
    }
}
