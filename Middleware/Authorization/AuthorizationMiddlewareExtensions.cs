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
        public static void UseAuthorization(this IMiniAppBuilder appBuilder, AuthorizationOptions? options = default)
        {
            options ??= new AuthorizationOptions();


            appBuilder.UseMiddleware<AuthorizationMiddleware>();
        }
    }
}
