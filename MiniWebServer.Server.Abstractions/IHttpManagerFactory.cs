namespace MiniWebServer.Server.Abstractions
{
    public interface IHttpManagerFactory
    {
        IHttpManager Create(ProtocolHandlerConfiguration config);
    }
}
