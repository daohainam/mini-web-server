using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.HttpParser.Http11;
using MiniWebServer.HttpParser.Http11;
using MiniWebServer.Server.ProtocolHandlers;
using MiniWebServer.Server.ProtocolHandlers.Http11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    public class ProtocolHandlerFactory: IProtocolHandlerFactory
    {
        public const int HTTP11 = 101;
        private readonly ILogger<ProtocolHandlerFactory> logger;
        private readonly IServiceProvider services;

        public ProtocolHandlerFactory(ILogger<ProtocolHandlerFactory> logger, IServiceProvider services)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IProtocolHandler Create(ProtocolHandlerConfiguration config)
        {
            if (config.ProtocolVersion == HTTP11)
            {
                return new Http11IProtocolHandler(config, services.GetService<ILogger<Http11IProtocolHandler>>(), services.GetService<IHttp11Parser>());
            }

            throw new ArgumentOutOfRangeException(nameof(config.ProtocolVersion), "Unknown protocol version");
        }
    }
}
