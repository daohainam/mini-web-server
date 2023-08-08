using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Authentication
{
    public static class AppContextAuthenticationExtensions
    {
        public static async Task SignInAsync(this IMiniAppContext context, ClaimsPrincipal principal)
        {
            context.User = principal;

            var authenticationServices = context.Services.GetServices<IAuthenticationService>();
            if (authenticationServices != null)
            {
                foreach (var authenticationService in authenticationServices)
                {
                    await authenticationService.SignInAsync(context, principal);
                }
            }

        }

        public static async Task SignOutAsync(this IMiniAppContext context)
        {
            context.User = null;

            var authenticationServices = context.Services.GetServices<IAuthenticationService>();
            if (authenticationServices != null)
            {
                foreach (var authenticationService in authenticationServices)
                {
                    await authenticationService.SignOutAsync(context);
                }
            }
        }
    }
}
