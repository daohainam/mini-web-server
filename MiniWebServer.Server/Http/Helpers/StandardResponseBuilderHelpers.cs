using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Http.Helpers
{
    public class StandardResponseBuilderHelpers
    {
        public static void InternalServerError(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.InternalServerError);
            builder.SetReasonPhrase("Internal Server Error");
        }

        public static void BadRequest(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.BadRequest);
            builder.SetReasonPhrase("Bad Request");
        }

        public static void OK(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.OK);
            builder.SetReasonPhrase("OK");
        }

        public static void NotFound(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.NotFound);
            builder.SetReasonPhrase("Not Found");
        }
    }
}
