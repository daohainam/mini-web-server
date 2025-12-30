using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.MiniApp.Builders;

namespace MiniWebServer.Authorization;

public static class AuthorizationMiddlewareExtensions
{
    public static void UseAuthorization(this IMiniAppBuilder appBuilder, Action<AuthorizationOptions>? optionAction = default)
    {
        var options = new AuthorizationOptions();
        optionAction?.Invoke(options);

        appBuilder.Services.AddTransient<IRouteMatcher>(services => new RegExRouteMatcher());

        appBuilder.Services.AddTransient(services => new AuthorizationMiddleware(
            options,
            services.GetRequiredService<IRouteMatcher>()
        ));

        appBuilder.UseMiddleware<AuthorizationMiddleware>();
    }
}
