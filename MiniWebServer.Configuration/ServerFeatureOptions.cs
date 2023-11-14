namespace MiniWebServer.Configuration
{
    public class ServerFeatureOptions
    {
        public int ReadBufferSize { get; init; } = 1024 * 8;
        public long MaxRequestBodySize { get; set; } = 1024 * 1024 * 10; // 10MB 
        public int ReadRequestTimeout { get; set; } = 6000;
        public int SendResponseTimeout { get; set; } = 300000;
        public int ConnectionTimeout { get; set; } = 180000;
    }
}
