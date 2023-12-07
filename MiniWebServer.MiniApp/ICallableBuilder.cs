namespace MiniWebServer.MiniApp
{
    public interface ICallableBuilder
    {
        ICallableBuilder AddFilter(ICallableFilter filter);
        ICallableBuilder AddFilter(Func<IMiniAppRequestContext, CancellationToken, bool> filter);
        ICallableBuilder AddFilter(Func<IMiniAppRequestContext, CancellationToken, Task<bool>> filter);
    }
}
