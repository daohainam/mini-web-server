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
            //var logger = loggerFactory != null ? loggerFactory.CreateLogger<AuthenticationMiddleware>() : NullLogger<AuthenticationMiddleware>.Instance;

            var jwtAuthenticationService = context.Services.GetService<JwtAuthenticationService>();
            if (jwtAuthenticationService != null)
            {
                var result = await jwtAuthenticationService.AuthenticateAsync(context);

                if (result != null) // authenticated successfully
                {
                    context.User = result.Principal;
                }
            }

            if (context.User == null)
            {
                var cookieAuthenticationService = context.Services.GetService<CookieAuthenticationService>();
                if (cookieAuthenticationService != null)
                {
                    var result = await cookieAuthenticationService.AuthenticateAsync(context);

                    if (result != null) // authenticated successfully
                    {
                        context.User = result.Principal;
                    }
                }
            }

            await next.InvokeAsync(context, cancellationToken);
        }
    }
}