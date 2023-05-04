using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
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
        private readonly ILogger logger;

        public ProtocolHandlerFactory(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IProtocolHandler Create(int protocolVersion)
        {
            if (protocolVersion == HTTP11)
            {
                return new Http11IProtocolHandler(logger, new RegexHttp11Parsers());
            }

            throw new ArgumentOutOfRangeException(nameof(protocolVersion), "Unknown protocol version");
        }
    }
}
