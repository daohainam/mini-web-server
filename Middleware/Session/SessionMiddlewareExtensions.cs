using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Session
{
    public static class SessionMiddlewareExtensions
    {
        public static void UseSession(this IMiniAppBuilder appBuilder, SessionOptions? options = default)
        {
            options ??= new SessionOptions();

            appBuilder.Services.AddTransient<ISessionIdGenerator>(services => new SessionIdGenerator());

            appBuilder.Services.AddTransient(services => new SessionMiddleware(
                options,
                services.GetService<ISessionIdGenerator>(),
                services.GetService<ISessionStore>() ?? new DistributedCacheSessionStore( // if no ISessionStore registered, we will use DistributedCacheSessionStore
                    services.GetService<IDistributedCache>(), 
                    services.GetService<ILoggerFactory>(), 
                    new DistributedCacheSessionStoreOptions()
                    )
                ));

            appBuilder.UseMiddleware<SessionMiddleware>();
        }
    }
}
