namespace MiniWebServer.MiniApp;

internal class CallableActionDelegate(string route, ICallable requestDelegate, params Abstractions.Http.HttpMethod[] httpMethods) : ICallableBuilder
{
    public string Route { get; } = route ?? throw new ArgumentNullException(nameof(route));
    public ICallable RequestDelegate { get; private set; } = requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate));
    public Abstractions.Http.HttpMethod[] HttpMethods { get; } = httpMethods ?? throw new ArgumentNullException(nameof(httpMethods));

    public ICallableBuilder AddFilter(ICallableFilter filter)
    {
        RequestDelegate = new FilteredRequestDelegate(RequestDelegate, filter);

        return this;
    }

    public ICallableBuilder AddFilter(Func<IMiniAppRequestContext, CancellationToken, bool> filter)
    {
        RequestDelegate = new FilteredRequestDelegate(RequestDelegate, new RequestDelegateCallableFilter(filter));

        return this;
    }

    public ICallableBuilder AddFilter(Func<IMiniAppRequestContext, CancellationToken, Task<bool>> filter)
    {
        RequestDelegate = new FilteredRequestDelegate(RequestDelegate, new RequestDelegateAsyncCallableFilter(filter));

        return this;
    }

    public bool IsMatched(string route, Abstractions.Http.HttpMethod httpMethod)
    {
        if (HttpMethods.Length > 0)
        {
            if (!HttpMethods.Contains(httpMethod))
                return false;
        }

        return Route.Equals(route); // todo: we should use pattern matching here and it is better to move IsMatched to a service class
    }
}
