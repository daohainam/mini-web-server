namespace MiniWebServer.Server.Abstractions
{
    public class ProtocolHandlerConfiguration(int protocolVersion, long maxRequestBodySize)
    {
        public int ProtocolVersion { get; } = protocolVersion;
        public long MaxRequestBodySize { get; } = maxRequestBodySize;
    }
}
