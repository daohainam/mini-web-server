namespace MiniWebServer.MiniApp;

public interface IMiniApp : ICallableService, ICallable
{
    ICallableBuilder Map(string route, ICallable action, params Abstractions.Http.HttpMethod[] methods);
}
