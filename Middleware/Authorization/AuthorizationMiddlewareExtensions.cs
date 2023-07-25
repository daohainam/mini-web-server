using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authorization
{
    public static class AuthorizationMiddlewareExtensions
    {
        public static void UseAuthorization(this IMiniAppBuilder appBuilder, Action<AuthorizationOptions>? optionAction = default)
        {
            var options = new AuthorizationOptions();
            if (optionAction != null)
            {
                optionAction(options);
            }

            appBuilder.Services.AddTransient(services => new AuthorizationMiddleware(
                options
            ));

            appBuilder.UseMiddleware<AuthorizationMiddleware>();
        }
    }
}
