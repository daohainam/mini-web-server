namespace MiniWebServer.MiniApp
{
    public interface IMiddleware
    {
        Task InvokeAsync(IMiniAppRequestContext context, ICallable next, CancellationToken cancellationToken = default);
    }
}
