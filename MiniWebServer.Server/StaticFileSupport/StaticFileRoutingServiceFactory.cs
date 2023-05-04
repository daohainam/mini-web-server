using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.StaticFileSupport
{
    public class StaticFileRoutingServiceFactory : IRoutingServiceFactory
    {
        private readonly ILogger logger;
        public StaticFileRoutingServiceFactory(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IRoutingService Create(string root)
        {
            var routingService = new StaticFileRoutingService(new DirectoryInfo(root), logger);

            return routingService;
        }
    }
}
