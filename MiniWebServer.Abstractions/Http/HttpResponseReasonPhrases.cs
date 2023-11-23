namespace MiniWebServer.Abstractions.Http
{
    public class HttpResponseReasonPhrases
    {
        private static readonly Dictionary<HttpResponseCodes, string> phrases = new()
        {
            { HttpResponseCodes.SwitchingProtocols, "Switching Protocols"},
            { HttpResponseCodes.OK, "OK"},
            { HttpResponseCodes.PartialContent, "Partial Content"},
            { HttpResponseCodes.TemporaryRedirect, "Temporary Redirect"},
            { HttpResponseCodes.NotFound, "Not Found"},
            { HttpResponseCodes.BadRequest, "Bad Request"},
            { HttpResponseCodes.Unauthorized, "Unauthorized"},
            { HttpResponseCodes.Forbidden, "Forbidden"},
            { HttpResponseCodes.MethodNotAllowed, "Method Not Allowed"},
            { HttpResponseCodes.NotImplemented, "Not Implemented"},
            { HttpResponseCodes.InternalServerError, "Internal Server Error"}
        };

        public static string GetReasonPhrase(HttpResponseCodes code)
        {
            if (phrases.TryGetValue(code, out string? v))
            {
                return v;
            }

            return code.ToString();
        }
    }
}
