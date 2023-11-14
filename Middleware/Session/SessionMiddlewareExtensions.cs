using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;

namespace MiniWebServer.Session
{
    public static class SessionMiddlewareExtensions
    {
        public static void AddSessionService(this IServiceCollection services, Action<SessionOptions>? action = default)
        {
            var options = new SessionOptions();
            action?.Invoke(options);

            services.TryAddTransient<ISessionIdGenerator>(services => new SessionIdGenerator());

            services.TryAddTransient(services => new SessionMiddleware(
                options,
                services.GetService<ISessionIdGenerator>(),
                services.GetService<ISessionStore>() ?? new DistributedCacheSessionStore( // if no ISessionStore registered, we will use DistributedCacheSessionStore
                    services.GetService<IDistributedCache>(),
                    services.GetService<ILoggerFactory>(),
                    new DistributedCacheSessionStoreOptions()
                    )
                ));
        }

        public static void UseSession(this IMiniAppBuilder appBuilder)
        {
            appBuilder.UseMiddleware<SessionMiddleware>();
        }
    }
}
