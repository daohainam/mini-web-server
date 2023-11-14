namespace MiniWebServer.MiniApp
{
    internal class ActionDelegate : ICallableBuilder
    {
        public ActionDelegate(string route, ICallable requestDelegate, params Abstractions.Http.HttpMethod[] httpMethods)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            RequestDelegate = requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate));
            HttpMethods = httpMethods ?? throw new ArgumentNullException(nameof(httpMethods));
        }

        public string Route { get; }
        public ICallable RequestDelegate { get; private set; }
        public Abstractions.Http.HttpMethod[] HttpMethods { get; }

        public ICallableBuilder AddFilter(ICallableFilter filter)
        {
            RequestDelegate = new FilteredRequestDelegate(RequestDelegate, filter);

            return this;
        }

        public ICallableBuilder AddFilter(Func<IMiniAppContext, CancellationToken, bool> filter)
        {
            RequestDelegate = new FilteredRequestDelegate(RequestDelegate, new RequestDelegateCallableFilter(filter));

            return this;
        }

        public ICallableBuilder AddFilter(Func<IMiniAppContext, CancellationToken, Task<bool>> filter)
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
}
