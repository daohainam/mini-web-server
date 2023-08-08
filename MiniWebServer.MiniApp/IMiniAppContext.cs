using MiniWebServer.Abstractions;
using System.Security.Claims;

namespace MiniWebServer.MiniApp
{
    public interface IMiniAppContext
    {
        IHttpRequest Request { get; }
        IHttpResponse Response { get; }
        ISession Session { get; set; }
        IServiceProvider Services { get; }
        ClaimsPrincipal? User { get; set; }

    }
}
