namespace MiniWebServer.MiniApp
{
    public interface IMiddleware
    {
        Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default);
    }
}
