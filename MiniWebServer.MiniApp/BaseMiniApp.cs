using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public class BaseMiniApp : IMiniApp
    {
        // todo: typical web apps will have may be hundred of routes, so this route table should be optimized for searching (can we use a B-Tree here?)
        // (update after checking https://github.com/microsoft/referencesource/blob/master/mscorlib/system/collections/generic/dictionary.cs#L297)
        // Dictionary is based on checksum and it is good enough to use here, we just need to partition by method (or may be by request segments also?)

        private readonly IDictionary<string, ActionDelegate> routes = new Dictionary<string, ActionDelegate>();

        public void Map(string route, RequestDelegate action, params Abstractions.Http.HttpMethod[] methods)
        {
            if (!methods.Any())
            {
                return;
            }

            var r = new ActionDelegate(route, action, methods);

            if (routes.ContainsKey(route))
                routes[route] = r;
            else
                routes.Add(route, r);
        }
        public void MapAll(string route, RequestDelegate action)
        {
            Map(route, action, 
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


        public void MapGet(string route, RequestDelegate action)
        {
            Map(route, action, Abstractions.Http.HttpMethod.Get);
        }
        public void MapPost(string route, RequestDelegate action)
        {
            Map(route, action, Abstractions.Http.HttpMethod.Post);
        }
        public void MapHead(string route, RequestDelegate action)
        {
            Map(route, action, Abstractions.Http.HttpMethod.Head);
        }
        public void MapPut(string route, RequestDelegate action)
        {
            Map(route, action, Abstractions.Http.HttpMethod.Put);
        }
        public void MapOptions(string route, RequestDelegate action)
        {
            Map(route, action, Abstractions.Http.HttpMethod.Options);
        }
        public void MapDelete(string route, RequestDelegate action)
        {
            Map(route, action, Abstractions.Http.HttpMethod.Delete);
        }
        public void MapConnect(string route, RequestDelegate action)
        {
            Map(route, action, Abstractions.Http.HttpMethod.Connect);
        }
        public void MapTrace(string route, RequestDelegate action)
        {
            Map(route, action, Abstractions.Http.HttpMethod.Trace);
        }
        public void MapGetAndPost(string route, RequestDelegate action)
        {
            Map(route, action, Abstractions.Http.HttpMethod.Get, Abstractions.Http.HttpMethod.Post);
        }

        public virtual ICallable? Find(IMiniAppRequest request)
        {
            foreach (var action in routes.Values)
            {
                if (action.IsMatched(request.Url, request.Method))
                {
                    return new ActionDelegateCallable(action);
                }
            }

            return null;
        }
    }
}
