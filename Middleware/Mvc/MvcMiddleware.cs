using MiniWebServer.MiniApp;

namespace MiniWebServer.Mvc
{
    public class MvcMiddleware : IMiddleware
    {
        public Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}