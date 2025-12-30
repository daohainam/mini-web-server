using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MiniWebServer.MiniApp;

internal class ActionDelegateCallable(CallableActionDelegate action, ILogger<ActionDelegateCallable> logger, ICallable? parent = default) : BaseCallable
{
    private readonly CallableActionDelegate action = action ?? throw new ArgumentNullException(nameof(action));
    private readonly ILogger<ActionDelegateCallable> logger = logger ?? NullLogger<ActionDelegateCallable>.Instance;

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
