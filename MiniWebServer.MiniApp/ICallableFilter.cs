namespace MiniWebServer.MiniApp
{
    public interface ICallableFilter
    {
        Task<bool> InvokeAsync(IMiniAppContext context, CancellationToken cancellationToken);
    }
}
