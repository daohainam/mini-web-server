using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Host
{
    public class HostContainer
    {
        public HostContainer(string host, DirectoryInfo directory, IRoutingService routingService)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            RoutingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
        }

        public string Host { get; }
        public DirectoryInfo Directory { get; }
        public IRoutingService RoutingService { get; }
    }
}
