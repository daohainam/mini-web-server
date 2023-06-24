using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.Authentication;
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
        public static void UseAuthentication(this IMiniAppBuilder appBuilder, AuthenticationOptions? options = default)
        {
            options ??= new AuthenticationOptions();


            appBuilder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
