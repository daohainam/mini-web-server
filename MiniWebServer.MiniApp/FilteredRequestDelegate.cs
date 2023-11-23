namespace MiniWebServer.MiniApp
{
    internal class FilteredRequestDelegate : ICallable
    {
        private readonly ICallable requestDelegate;
        private readonly ICallableFilter filter;

        public FilteredRequestDelegate(ICallable requestDelegate, ICallableFilter filter)
        {
            this.requestDelegate = requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate));
            this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public async Task InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken = default)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (await filter.InvokeAsync(context, cancellationToken))
            {
                await requestDelegate.InvokeAsync(context, cancellationToken);
            }
        }
    }
}