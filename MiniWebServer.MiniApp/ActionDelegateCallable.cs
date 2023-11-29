using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MiniWebServer.MiniApp
{
    internal class ActionDelegateCallable : BaseCallable
    {
        private readonly CallableActionDelegate action;
        private readonly ILogger<ActionDelegateCallable> logger;
        private readonly ICallable? parent;

        public ActionDelegateCallable(CallableActionDelegate action, ILogger<ActionDelegateCallable> logger, ICallable? parent = default)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.logger = logger ?? NullLogger<ActionDelegateCallable>.Instance;
            this.parent = parent;
        }

        public override async Task InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await action.RequestDelegate.InvokeAsync(context, cancellationToken);
            } catch (Exception ex)
            {
                logger.LogError(ex, "Error handling action");
            }

            // this is an endpoint so we don't call it's parent
            //if (parent != null)
            //{
            //    await parent.InvokeAsync(context, cancellationToken);
            //}
        }
    }
}
