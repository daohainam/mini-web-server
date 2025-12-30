using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Authentication;
using MiniWebServer.MiniApp.Builders;

namespace MiniWebServer.Authentication;

public static class AuthenticationMiddlewareExtensions
{
    public static IMiniAppBuilder UseAuthentication(this IMiniAppBuilder appBuilder, Action<AuthenticationOptions>? optionAction = default)
    {
        var options = new AuthenticationOptions();
        optionAction?.Invoke(options);

        appBuilder.Services.AddTransient<IPrincipalStore>(services => new MemoryPrincipalStore());

        appBuilder.Services.AddTransient(services => new AuthenticationMiddleware(
            options
            ));

        appBuilder.UseMiddleware<AuthenticationMiddleware>();

        return appBuilder;
    }

    public static IMiniAppBuilder UseCookieAuthentication(this IMiniAppBuilder appBuilder, CookieAuthenticationOptions? options = null)
    {
        appBuilder.Services.AddTransient<IAuthenticationService>(services => new CookieAuthenticationService(options ?? new CookieAuthenticationOptions(),
            new DefaultPrincipalKeyGenerator(),
            services.GetService<ILoggerFactory>()
            ));

        return appBuilder;
    }

    public static IMiniAppBuilder UseJwtAuthentication(this IMiniAppBuilder appBuilder, JwtAuthenticationOptions? options = null)
    {
        appBuilder.Services.AddTransient<IAuthenticationService>(services => new JwtAuthenticationService(options ?? new JwtAuthenticationOptions(
            new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            ),
            services.GetService<ILoggerFactory>()
            ));

        return appBuilder;
    }
}
