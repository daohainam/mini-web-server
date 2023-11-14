namespace MiniWebServer.Server
{
    public interface IRequestIdManager
    {
        ulong GetNext();
    }
}
