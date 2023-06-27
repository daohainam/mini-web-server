using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authentication;

namespace MiniWebServer.Authorization
{
    public class AuthorizationMiddleware : IMiddleware
    {
        private AuthorizationOptions options;

        public AuthorizationMiddleware(AuthorizationOptions options)
        {
            this.options = options ?? new();
        }

        public Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}