using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    public class HttpResponseReasonPhrases
    {
        public static readonly IReadOnlyDictionary<HttpResponseCodes, string> ReasonPhrases = new Dictionary<HttpResponseCodes, string>() {
            { HttpResponseCodes.OK, "OK" },
            { HttpResponseCodes.BadRequest, "Bad Request" },
            { HttpResponseCodes.Forbidden, "Forbidden" },
            { HttpResponseCodes.NotFound, "Not Found" },
            { HttpResponseCodes.InternalServerError, "Internal Server Error" },
        };
    }
}
