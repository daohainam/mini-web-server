using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MiniWebServer.MiniApp;

public class BaseMiniApp(ServiceProvider services, IEnumerable<IMiddleware> middlewareChain) : IMiniApp
{
    // todo: typical web apps will have may be hundred of routes, so this route table should be optimized for searching (can we use a B-Tree here?)
    // (update after checking https://github.com/microsoft/referencesource/blob/master/mscorlib/system/collections/generic/dictionary.cs#L297)
    // Dictionary is based on checksum and it is good enough to use here, we just need to partition by method (or may be by request segments also?)

    private readonly Dictionary<string, CallableActionDelegate> callableEndpoints = [];
    private readonly ServiceProvider services = services ?? throw new ArgumentNullException(nameof(services));
    private readonly IEnumerable<IMiddleware> middlewareChain = middlewareChain ?? throw new ArgumentNullException(nameof(middlewareChain));

    public ICallableBuilder Map(string route, ICallable action, params Abstractions.Http.HttpMethod[] methods)
    {
        if (methods.Length == 0)
        {
            throw new ArgumentException("Methods required", nameof(methods));
        }

        var r = new CallableActionDelegate(route, action, methods);

        if (!callableEndpoints.TryAdd(route, r))
            callableEndpoints[route] = r;
        return r;
    }

    public virtual ICallable? Find(IMiniAppRequestContext context)
    {
        ICallable callable = this;

        foreach (var action in callableEndpoints.Values)
        {
            if (action.IsMatched(context.Request.Url, context.Request.Method))
            {
                callable = new ActionDelegateCallable(action, services.GetRequiredService<ILogger<ActionDelegateCallable>>(), callable);
                break; // we need only one endpoint
            }
        }

        foreach (var middleware in middlewareChain)
        {
            var callWrapper = new MiddlewareWrapper(middleware, callable);

            callable = callWrapper;
        }

        return callable;
    }

    public Task InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = Abstractions.HttpResponseCodes.NotFound;

        return Task.CompletedTask;
    }

    private class MiddlewareWrapper(IMiddleware middleware, ICallable next) : ICallable
    {
        public IMiddleware Middleware { get; init; } = middleware ?? throw new ArgumentNullException(nameof(middleware));
        public ICallable Next { get; init; } = next ?? throw new ArgumentNullException(nameof(next));

        public Task InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken = default)
        {
            return Middleware.InvokeAsync(context, Next, cancellationToken);
        }
    }
}
