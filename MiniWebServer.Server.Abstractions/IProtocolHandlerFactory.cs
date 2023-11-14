namespace MiniWebServer.Server.Abstractions
{
    public interface IProtocolHandlerFactory
    {
        IProtocolHandler Create(ProtocolHandlerConfiguration config);
    }
}
