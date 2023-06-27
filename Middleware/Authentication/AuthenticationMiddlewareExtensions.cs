using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Authentication;
using MiniWebServer.MiniApp.Authentication;
using MiniWebServer.MiniApp.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Session
{
    public static class AuthenticationMiddlewareExtensions
    {
        public static IMiniAppBuilder UseAuthentication(this IMiniAppBuilder appBuilder, AuthenticationOptions? options = default)
        {
            options ??= new AuthenticationOptions();

            appBuilder.Services.AddTransient(services => new AuthenticationMiddleware(
                options
                ));

            appBuilder.UseMiddleware<AuthenticationMiddleware>();

            return appBuilder;
        }

        public static void UseCookieAuthentication(this IMiniAppBuilder appBuilder, CookieAuthenticationOptions? options = null)
        {
            appBuilder.Services.AddTransient<IAuthenticationService>(services => new CookieAuthenticationService(options ?? new CookieAuthenticationOptions(), 
                services.GetService<ILoggerFactory>()
                ));
        }
    }
}
