using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Http.Helpers
{
    public class StandardResponseBuilderHelpers
    {
        public static void NotFound(IHttpResponse response)
        {
            response.StatusCode = HttpResponseCodes.NotFound;
            response.Headers.ContentLength = 0;
            response.ReasonPhrase = "Not Found";
        }
    }
}
