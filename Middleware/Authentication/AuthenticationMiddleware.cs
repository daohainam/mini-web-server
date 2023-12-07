using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authentication;

namespace MiniWebServer.Authentication
{
    public class AuthenticationMiddleware(AuthenticationOptions options) : IMiddleware
    {
        public async Task InvokeAsync(IMiniAppRequestContext context, ICallable next, CancellationToken cancellationToken = default)
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