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

        internal readonly IDictionary<string, ActionDelegate> routes = new Dictionary<string, ActionDelegate>();

        public void MapGet(string route, RequestDelegate action)
        {
            var r = new ActionDelegate(route, action, Abstractions.Http.HttpMethod.Get);

            if (routes.ContainsKey(route))
                routes[route] = r;
            else
                routes.Add(route, r);
        }

        public void MapPost(string route, RequestDelegate action)
        {
            var r = new ActionDelegate(route, action, Abstractions.Http.HttpMethod.Post);

            if (routes.ContainsKey(route))
                routes[route] = r;
            else
                routes.Add(route, r);
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
