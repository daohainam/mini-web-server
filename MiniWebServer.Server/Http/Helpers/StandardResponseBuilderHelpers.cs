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
        public static void InternalServerError(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.InternalServerError);
            builder.SetReasonPhrase("Internal Server Error");
        }

        public static void NotImplemented(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.NotImplemented);
            builder.SetReasonPhrase("Not Implemented");
        }

        public static void BadRequest(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.BadRequest);
            builder.SetReasonPhrase("Bad Request");
        }
        public static void MethodNotAllowed(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.MethodNotAllowed);
            builder.SetReasonPhrase("Method Not Allowed");
        }

        public static void OK(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.OK);
            builder.SetReasonPhrase("OK");
        }

        public static void NotFound(IHttpResponseBuilder builder)
        {
            builder.SetStatusCode(System.Net.HttpStatusCode.NotFound);
            builder.AddHeader("Content-Length", "0");
            builder.SetReasonPhrase("Not Found");
        }
    }
}
