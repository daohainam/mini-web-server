using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.HttpParser.Http11;
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
        private readonly ILoggerFactory loggerFactory;
        private readonly IServiceProvider services;

        public ProtocolHandlerFactory(ILoggerFactory loggerFactory, IServiceProvider services)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IProtocolHandler Create(ProtocolHandlerConfiguration config)
        {
            if (config.ProtocolVersion == HTTP11)
            {
                return new Http11IProtocolHandler(config, loggerFactory, services.GetService<IHttp11Parser>());
            }

            throw new ArgumentOutOfRangeException(nameof(config.ProtocolVersion), "Unknown protocol version");
        }
    }
}
