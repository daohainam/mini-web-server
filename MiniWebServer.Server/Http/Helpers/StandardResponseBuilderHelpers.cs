using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Http.Helpers
{
    public class StandardResponseBuilderHelpers
    {
        public static void NotFound(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(global::MiniWebServer.Abstractions.HttpResponseCodes.NotFound);
            builder.AddHeader("Content-Length", "0");
            builder.SetReasonPhrase("Not Found");
        }
    }
}
