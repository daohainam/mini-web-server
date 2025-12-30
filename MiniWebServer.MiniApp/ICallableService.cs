namespace MiniWebServer.MiniApp;

public interface ICallableService
{
    ICallable? Find(IMiniAppRequestContext request);
}
