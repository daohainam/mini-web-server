using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.WebSocket.Abstractions;
using System.Security.Claims;

namespace MvcMiddlewareTests
{
    internal class FakeMiniAppContext : IMiniAppRequestContext
    {
        private readonly IHttpRequest request;

        public FakeMiniAppContext(Func<IHttpRequest> request)
        {
            this.request = request();
        }

        public IHttpRequest Request => request;

        public IHttpResponse Response => throw new NotImplementedException();

        public ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IServiceProvider Services => throw new NotImplementedException();

        public ClaimsPrincipal? User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        IWebSocketManager IMiniAppContext.WebSockets { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
