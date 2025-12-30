namespace MiniWebServer.MiniApp;

internal class RequestDelegateAsyncCallableFilter(Func<IMiniAppRequestContext, CancellationToken, Task<bool>> filter) : ICallableFilter
{
    public async Task<bool> InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        return await filter(context, cancellationToken);
    }
}
