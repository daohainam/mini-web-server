namespace MiniWebServer.MiniApp
{
    internal class FilteredRequestDelegate(ICallable requestDelegate, ICallableFilter filter) : ICallable
    {
        private readonly ICallable requestDelegate = requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate));
        private readonly ICallableFilter filter = filter ?? throw new ArgumentNullException(nameof(filter));

        public async Task InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (await filter.InvokeAsync(context, cancellationToken))
            {
                await requestDelegate.InvokeAsync(context, cancellationToken);
            }
        }
    }
}