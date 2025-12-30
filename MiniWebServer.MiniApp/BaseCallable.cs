namespace MiniWebServer.MiniApp;

public abstract class BaseCallable : ICallable
{
    public virtual Task InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken = default)
    {
        // by default we send 404 Not Found
        context.Response.StatusCode = Abstractions.HttpResponseCodes.NotFound;

        return Task.CompletedTask;
    }
}
