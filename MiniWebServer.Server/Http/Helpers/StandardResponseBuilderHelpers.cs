using MiniWebServer.Abstractions;

namespace MiniWebServer.Server.Http.Helpers
{
    public class StandardResponseBuilderHelpers
    {
        public static void NotFound(IHttpResponse response)
        {
            response.StatusCode = HttpResponseCodes.NotFound;
            response.Headers.ContentLength = 0;
        }
    }
}
