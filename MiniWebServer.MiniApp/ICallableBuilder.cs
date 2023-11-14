namespace MiniWebServer.MiniApp
{
    public interface ICallableBuilder
    {
        ICallableBuilder AddFilter(ICallableFilter filter);
        ICallableBuilder AddFilter(Func<IMiniAppContext, CancellationToken, bool> filter);
        ICallableBuilder AddFilter(Func<IMiniAppContext, CancellationToken, Task<bool>> filter);
    }
}
