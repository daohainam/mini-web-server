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
            var authenticationServices = context.Services.GetServices<IAuthenticationService>();
            if (authenticationServices != null)
            {
                foreach (var authenticationService in authenticationServices)
                {
                    var result = await authenticationService.AuthenticateAsync(context);

                    if (result.IsSucceeded) // authenticated successfully
                    {
                        context.User = result.Principal;
                        break;
                    }
                }
            }

            await next.InvokeAsync(context, cancellationToken);
        }
    }
}