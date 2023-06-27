using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authentication;

namespace MiniWebServer.Authentication
{
    public class AuthenticationMiddleware : IMiddleware
    {
        private AuthenticationOptions options;

        public AuthenticationMiddleware(AuthenticationOptions options)
        {
            this.options = options ?? new();
        }

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            var authenticationService = context.Services.GetService<IAuthenticationService>() ?? throw new InvalidOperationException("IAuthenticationService required");

            var result = await authenticationService.AuthenticateAsync(context);

            if (result != null) // authenticated successfully
            {
                context.User = result.Principal;
            }

            await next.InvokeAsync(context, cancellationToken);
        }
    }
}