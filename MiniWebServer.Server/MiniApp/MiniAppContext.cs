using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.WebSocket.Abstractions;
using System.Security.Claims;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniAppContext : IMiniAppRequestContext
    {
        public MiniAppContext(MiniAppConnectionContext connectionContext, IMiniApp app, IHttpRequest request, IHttpResponse response, ISession session, ClaimsPrincipal? user)
        {
            ConnectionContext = connectionContext ?? throw new ArgumentNullException(nameof(connectionContext));
            App = app ?? throw new ArgumentNullException(nameof(app));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
            Session = session;
            User = user;
        }

        private MiniAppConnectionContext ConnectionContext { get; }

        public IMiniApp App { get; }
        public IHttpRequest Request { get; }
        public IHttpResponse Response { get; }
        public ISession Session { get; set; } // session can be changed by session middleware
        public IServiceProvider Services => ConnectionContext.Services;
        public ClaimsPrincipal? User { get; set; }
        public IWebSocketManager WebSockets
        {
            get
            {
                return ConnectionContext.WebSockets;
            }

            set 
            { 
                ConnectionContext.WebSockets = value; 
            }
        }
    }
}
