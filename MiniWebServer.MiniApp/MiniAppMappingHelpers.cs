using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public static class MiniAppMappingHelpers
    {
        public static void MapAll(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action,
                Abstractions.Http.HttpMethod.Get,
                Abstractions.Http.HttpMethod.Post,
                Abstractions.Http.HttpMethod.Put,
                Abstractions.Http.HttpMethod.Head,
                Abstractions.Http.HttpMethod.Options,
                Abstractions.Http.HttpMethod.Delete,
                Abstractions.Http.HttpMethod.Connect,
                Abstractions.Http.HttpMethod.Trace
                );
        }

        public static void MapGet(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action, Abstractions.Http.HttpMethod.Get);
        }
        public static void MapGet(this IMiniApp app, string route, RequestDelegate action)
        {
            app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Get);
        }
        public static void MapPost(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action, Abstractions.Http.HttpMethod.Post);
        }
        public static void MapPost(this IMiniApp app, string route, RequestDelegate action)
        {
            app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Post);
        }
        public static void MapHead(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action, Abstractions.Http.HttpMethod.Head);
        }
        public static void MapPut(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action, Abstractions.Http.HttpMethod.Put);
        }
        public static void MapOptions(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action, Abstractions.Http.HttpMethod.Options);
        }
        public static void MapDelete(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action, Abstractions.Http.HttpMethod.Delete);
        }
        public static void MapConnect(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action, Abstractions.Http.HttpMethod.Connect);
        }
        public static void MapTrace(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action, Abstractions.Http.HttpMethod.Trace);
        }
        public static void MapGetAndPost(this IMiniApp app, string route, ICallable action)
        {
            app.Map(route, action, Abstractions.Http.HttpMethod.Get, Abstractions.Http.HttpMethod.Post);
        }

        private class CallableWrapper : ICallable
        {
            private readonly RequestDelegate action;

            public CallableWrapper(RequestDelegate action)
            {
                this.action = action ?? throw new ArgumentNullException(nameof(action));
            }

            public Task InvokeAsync(IMiniAppContext context, CancellationToken cancellationToken = default)
            {
                context.Response.StatusCode = HttpResponseCodes.OK;
                context.Response.ReasonPhrase = HttpResponseReasonPhrases.GetReasonPhrase(HttpResponseCodes.OK);
                return action(context, cancellationToken);
            }
        }
    }
}
