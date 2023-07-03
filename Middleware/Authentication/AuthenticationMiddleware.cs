using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authentication;

namespace MiniWebServer.Authentication
{
    public class AuthenticationMiddleware : IMiddleware
    {
        private readonly AuthenticationOptions options;

        public AuthenticationMiddleware(AuthenticationOptions options)
        {
            this.options = options ?? new();
        }

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            var loggerFactory = context.Services.GetService<ILoggerFactory>();
            var logger = loggerFactory != null ? loggerFactory.CreateLogger<AuthenticationMiddleware>() : NullLogger<AuthenticationMiddleware>.Instance;

            if (options.JwtAuthenticationOptions != null)
            {
                var authenticationService = context.Services.GetService<JwtAuthenticationService>();
                if (authenticationService == null)
                {
                    logger.LogWarning("JwtAuthenticationService not registered");
                }
                else
                {
                    var result = await authenticationService.AuthenticateAsync(context);

                    if (result != null) // authenticated successfully
                    {
                        context.User = result.Principal;
                    }
                }
            }

            if (context.User == null)
            {
                if (options.CookieAuthenticationOptions != null)
                {
                    var authenticationService = context.Services.GetService<CookieAuthenticationService>();
                    if (authenticationService == null)
                    {
                        logger.LogWarning("CookieAuthenticationService not registered");
                    }
                    else
                    {
                        var result = await authenticationService.AuthenticateAsync(context);

                        if (result != null) // authenticated successfully
                        {
                            context.User = result.Principal;
                        }
                    }
                }
            }

            await next.InvokeAsync(context, cancellationToken);
        }
    }
}