using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public class HttpResponseReasonPhrases
    {
        public static readonly IReadOnlyDictionary<int, string> ReasonPhrases = new Dictionary<int, string>() {
            { HttpResponseCodes.Ok, "OK" },
            { HttpResponseCodes.Forbidden, "Forbidden" },
            { HttpResponseCodes.NotFound, "Not Found" },
            { HttpResponseCodes.InternalServerError, "Internal Server Error" },
        };
    }
}
