//using Microsoft.Extensions.Logging;
//using MiniWebServer.Abstractions;
//using MiniWebServer.Server.StaticFileSupport;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace MiniWebServer.MiniApp.Web.StaticFileSupport
//{
//    // Abstract Factory pattern
//    public class StaticFileCallableServiceFactory : IRoutingServiceFactory
//    {
//        private readonly ILogger logger;
//        public StaticFileCallableServiceFactory(ILogger logger)
//        {
//            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        public IRoutingService Create(string root)
//        {
//            var routingService = new StaticFileCallableService(new DirectoryInfo(root), logger);

//            return routingService;
//        }
//    }
//}
