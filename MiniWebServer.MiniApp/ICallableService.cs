namespace MiniWebServer.MiniApp
{
    public interface ICallableService
    {
        ICallable? Find(IMiniAppContext request);
    }
}
