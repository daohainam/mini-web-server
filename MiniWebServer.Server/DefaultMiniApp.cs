using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.MiniApp;

namespace MiniWebServer.Server
{
    internal class DefaultMiniApp : BaseMiniApp
    {
        public DefaultMiniApp(ServiceProvider services) : base(services, Array.Empty<IMiddleware>())
        {
        }
    }
}
