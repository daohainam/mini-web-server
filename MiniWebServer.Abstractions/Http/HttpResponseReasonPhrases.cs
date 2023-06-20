using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpResponseReasonPhrases
    {
        private static readonly Dictionary<HttpResponseCodes, string> phrases = new()
        {
            { HttpResponseCodes.OK, "OK"},
            { HttpResponseCodes.NotFound, "Not Found"},
            { HttpResponseCodes.BadRequest, "Bad Request"},
            { HttpResponseCodes.Forbidden, "Forbidden"},
            { HttpResponseCodes.MethodNotAllowed, "Method Not Allowed"},
            { HttpResponseCodes.NotImplemented, "Not Implemented"},
            { HttpResponseCodes.InternalServerError, "Internal Server Error"}
        };

        public static string GetReasonPhrase(HttpResponseCodes code)
        {
            return phrases[code];
        }
    }
}
