namespace MiniWebServer.Server.Abstractions
{
    public class ProtocolHandlerConfiguration
    {
        public ProtocolHandlerConfiguration(int protocolVersion, long maxRequestBodySize)
        {
            ProtocolVersion = protocolVersion;
            MaxRequestBodySize = maxRequestBodySize;
        }
        public int ProtocolVersion { get; }
        public long MaxRequestBodySize { get; }
    }
}
