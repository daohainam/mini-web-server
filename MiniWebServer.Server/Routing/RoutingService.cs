using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.Server.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Routing
{
    internal class RoutingService : IRoutingService
    {
        private readonly List<IRoutingService> children = new();

        public ICallable? FindRoute(string url)
        {
            lock(children)
            {
                foreach (var child in children)
                {
                    var resource = child.FindRoute(url);

                    if (resource != null)
                    {
                        return resource;
                    }
                }
            }

            return null;
        }

        public void AddRoute(IRoutingService routingService) {
            lock (children)
            {
                children.Add(routingService);
            }
        }

        public void AddRoutes(IEnumerable<IRoutingService> routingServices)
        {
            lock (children)
            {
                children.AddRange(routingServices);
            }
        }
    }
}
