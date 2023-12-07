namespace MiniWebServer.MiniApp
{
    internal class RequestDelegateCallableFilter(Func<IMiniAppRequestContext, CancellationToken, bool> filter) : ICallableFilter
    {
        private readonly Func<IMiniAppRequestContext, CancellationToken, bool> filter = filter ?? throw new ArgumentNullException(nameof(filter));

        public async Task<bool> InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);

            return await Task.FromResult(filter(context, cancellationToken));
        }
    }
}
