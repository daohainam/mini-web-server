namespace MiniWebServer.Server.Abstractions
{
    public class ConnectionProcessResult
    {
        public bool Success { get; }
        public bool CloseConnectionRequested { get; }
        public int ProtocolVersionRequested { get; }
    }
}
