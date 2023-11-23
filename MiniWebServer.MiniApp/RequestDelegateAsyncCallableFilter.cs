namespace MiniWebServer.MiniApp
{
    internal class RequestDelegateAsyncCallableFilter : ICallableFilter
    {
        private readonly Func<IMiniAppRequestContext, CancellationToken, Task<bool>> filter;

        public RequestDelegateAsyncCallableFilter(Func<IMiniAppRequestContext, CancellationToken, Task<bool>> filter)
        {
            this.filter = filter;
        }

        public async Task<bool> InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return await filter(context, cancellationToken);
        }
    }
}
