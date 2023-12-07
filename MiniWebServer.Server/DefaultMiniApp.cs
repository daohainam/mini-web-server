using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.MiniApp;

namespace MiniWebServer.Server
{
    internal class DefaultMiniApp(ServiceProvider services) : BaseMiniApp(services, Array.Empty<IMiddleware>())
    {
    }
}
