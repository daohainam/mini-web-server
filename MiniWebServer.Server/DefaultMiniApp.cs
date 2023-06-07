using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    internal class DefaultMiniApp : BaseMiniApp
    {
        public DefaultMiniApp(ServiceProvider services) : base(services, Array.Empty<IMiddleware>())
        {
        }
    }
}
