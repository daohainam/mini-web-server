using MiniWebServer.Abstractions;
using MiniWebServer.WebSocket.Abstractions;
using System.Security.Claims;

namespace MiniWebServer.MiniApp;

public interface IMiniAppRequestContext: IMiniAppContext
{
    IHttpRequest Request { get; }
    IHttpResponse Response { get; }
}
