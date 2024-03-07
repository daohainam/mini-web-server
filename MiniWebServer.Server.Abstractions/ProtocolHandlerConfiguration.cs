using MiniWebServer.Abstractions;

namespace MiniWebServer.Server.Abstractions
{
    public class ProtocolHandlerConfiguration(HttpVersions protocolVersion, long maxRequestBodySize)
    {
        public HttpVersions ProtocolVersion { get; } = protocolVersion;
        public long MaxRequestBodySize { get; } = maxRequestBodySize;
    }
}
