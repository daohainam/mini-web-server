using MiniWebServer.Abstractions;

namespace MiniWebServer.Server.Abstractions;

public interface IProtocolHandlerFactory
{
    IProtocolHandler Create(HttpVersions httpVersion, ProtocolHandlerConfiguration config, ProtocolHandlerContext protocolHandlerContext);
}
