using MiniWebServer.Abstractions;

namespace MiniWebServer.MiniApp;

public static class MiniAppMappingHelpers
{
    public static ICallableBuilder Map(this IMiniApp app, string route, RequestDelegate action, params Abstractions.Http.HttpMethod[] methods)
    {
        return app.Map(route, new CallableWrapper(action),
            methods
            );
    }

    public static ICallableBuilder MapAll(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action,
            Abstractions.Http.HttpMethod.Get,
            Abstractions.Http.HttpMethod.Post,
            Abstractions.Http.HttpMethod.Put,
            Abstractions.Http.HttpMethod.Head,
            Abstractions.Http.HttpMethod.Options,
            Abstractions.Http.HttpMethod.Delete,
            Abstractions.Http.HttpMethod.Connect,
            Abstractions.Http.HttpMethod.Trace
            );
    }
    public static ICallableBuilder MapAll(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action),
            Abstractions.Http.HttpMethod.Get,
            Abstractions.Http.HttpMethod.Post,
            Abstractions.Http.HttpMethod.Put,
            Abstractions.Http.HttpMethod.Head,
            Abstractions.Http.HttpMethod.Options,
            Abstractions.Http.HttpMethod.Delete,
            Abstractions.Http.HttpMethod.Connect,
            Abstractions.Http.HttpMethod.Trace
            );
    }

    public static ICallableBuilder MapGet(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action, Abstractions.Http.HttpMethod.Get);
    }
    public static ICallableBuilder MapGet(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Get);
    }
    public static ICallableBuilder MapPost(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action, Abstractions.Http.HttpMethod.Post);
    }
    public static ICallableBuilder MapPost(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Post);
    }
    public static ICallableBuilder MapHead(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action, Abstractions.Http.HttpMethod.Head);
    }
    public static ICallableBuilder MapHead(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Head);
    }
    public static ICallableBuilder MapPut(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action, Abstractions.Http.HttpMethod.Put);
    }
    public static ICallableBuilder MapPut(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Put);
    }
    public static ICallableBuilder MapOptions(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action, Abstractions.Http.HttpMethod.Options);
    }
    public static ICallableBuilder MapOptions(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Options);
    }
    public static ICallableBuilder MapDelete(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action, Abstractions.Http.HttpMethod.Delete);
    }
    public static ICallableBuilder MapDelete(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Delete);
    }
    public static ICallableBuilder MapConnect(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action, Abstractions.Http.HttpMethod.Connect);
    }
    public static ICallableBuilder MapConnect(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Connect);
    }
    public static ICallableBuilder MapTrace(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action, Abstractions.Http.HttpMethod.Trace);
    }
    public static ICallableBuilder MapTrace(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Trace);
    }
    public static ICallableBuilder MapGetAndPost(this IMiniApp app, string route, ICallable action)
    {
        return app.Map(route, action, Abstractions.Http.HttpMethod.Get, Abstractions.Http.HttpMethod.Post);
    }
    public static ICallableBuilder MapGetAndPost(this IMiniApp app, string route, RequestDelegate action)
    {
        return app.Map(route, new CallableWrapper(action), Abstractions.Http.HttpMethod.Get, Abstractions.Http.HttpMethod.Post);
    }


    private class CallableWrapper(RequestDelegate action) : ICallable
    {
        private readonly RequestDelegate action = action ?? throw new ArgumentNullException(nameof(action));

        public Task InvokeAsync(IMiniAppRequestContext context, CancellationToken cancellationToken = default)
        {
            context.Response.StatusCode = HttpResponseCodes.OK;
            return action(context, cancellationToken);
        }
    }
}
