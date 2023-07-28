using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authentication;
using MiniWebServer.MiniApp.Authorization;

namespace MiniWebServer.Authorization
{
    public class AuthorizationMiddleware : IMiddleware
    {
        private readonly AuthorizationOptions options;
        private readonly IRouteMatcher routeMatcher;

        public AuthorizationMiddleware(AuthorizationOptions options, IRouteMatcher routeMatcher)
        {
            this.options = options ?? new();
            this.routeMatcher = routeMatcher;
        }

        public async Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default)
        {
            var url = context.Request.Url;
            var route = options.Routes.Keys.Where(r => routeMatcher.IsMatched(r, url)).FirstOrDefault();

            if (route != null)
            {
                var policyNames = options.Routes[route];

                if (policyNames != null)
                {
                    foreach (var policyName in policyNames)
                    {
                        if (!options.Policies.TryGetValue(policyName, out IPolicy? policy) || policy == null
                            )
                        {
                            // if policy not found then return 401 Unauthenticated
                            context.Response.StatusCode = Abstractions.HttpResponseCodes.Unauthorized;
                            return;
                        }

                        if (!policy.IsValid(context))
                        {
                            // if policy not found then return 401 Unauthenticated
                            context.Response.StatusCode = Abstractions.HttpResponseCodes.Unauthorized;
                            return;
                        }
                    }
                }
            }

            await next.InvokeAsync(context, cancellationToken);
        }
    }
}