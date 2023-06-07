using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    internal class ActionDelegate
    {
        public ActionDelegate(string route, ICallable requestDelegate, params Abstractions.Http.HttpMethod[] httpMethods)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            RequestDelegate = requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate));
            HttpMethods = httpMethods ?? throw new ArgumentNullException(nameof(httpMethods));
        }

        public string Route { get; }
        public ICallable RequestDelegate { get; }
        public Abstractions.Http.HttpMethod[] HttpMethods { get; }

        public bool IsMatched(string route, Abstractions.Http.HttpMethod httpMethod)
        {
            if (HttpMethods.Length > 0)
            {
                if (!HttpMethods.Contains(httpMethod))
                    return false;
            }

            return Route.Equals(route); // todo: we should use pattern matching here and it is better to move IsMatched to a service class
        }
    }
}
