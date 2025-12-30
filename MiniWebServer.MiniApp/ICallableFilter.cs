namespace MiniWebServer.MiniApp;

public interface ICallableFilter
{
    Task<bool> InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken);
}
