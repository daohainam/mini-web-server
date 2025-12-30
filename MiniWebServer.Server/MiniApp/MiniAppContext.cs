using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.WebSocket.Abstractions;
using System.Security.Claims;

namespace MiniWebServer.Server.MiniApp;

public class MiniAppContext(MiniAppConnectionContext connectionContext, IMiniApp app, IHttpRequest request, IHttpResponse response, ISession session, ClaimsPrincipal? user) : IMiniAppRequestContext
{
    private MiniAppConnectionContext ConnectionContext { get; } = connectionContext ?? throw new ArgumentNullException(nameof(connectionContext));

    public IMiniApp App { get; } = app ?? throw new ArgumentNullException(nameof(app));
    public IHttpRequest Request { get; } = request ?? throw new ArgumentNullException(nameof(request));
    public IHttpResponse Response { get; } = response ?? throw new ArgumentNullException(nameof(response));
    public ISession Session { get; set; } = session;
    public IServiceProvider Services => ConnectionContext.Services;
    public ClaimsPrincipal? User { get; set; } = user;
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
