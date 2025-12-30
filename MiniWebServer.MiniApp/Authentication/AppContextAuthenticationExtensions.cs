using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace MiniWebServer.MiniApp.Authentication;

public static class AppContextAuthenticationExtensions
{
    public static async Task SignInAsync(this IMiniAppRequestContext context, ClaimsPrincipal principal)
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

    public static async Task SignOutAsync(this IMiniAppRequestContext context)
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
