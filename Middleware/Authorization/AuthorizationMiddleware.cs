using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authorization;

namespace MiniWebServer.Authorization;

public class AuthorizationMiddleware(AuthorizationOptions options, IRouteMatcher routeMatcher) : IMiddleware
{
    private readonly AuthorizationOptions options = options ?? new();

    public async Task InvokeAsync(IMiniAppRequestContext context, ICallable next, CancellationToken cancellationToken = default)
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
                        // if not found then return 401 Unauthenticated
                        context.Response.StatusCode = Abstractions.HttpResponseCodes.Unauthorized;
                        return;
                    }

                    if (!policy.IsValid(context))
                    {
                        // if not valid then return 401 Unauthenticated
                        context.Response.StatusCode = Abstractions.HttpResponseCodes.Unauthorized;
                        return;
                    }
                }
            }
        }
        else
        {
            // by default we accept all requests except ones defined in Routes, if you want to deny-all, set options.NoMatchedRoute = (context) => false
            if (!options.NoMatchedRoute(context))
            {
                context.Response.StatusCode = Abstractions.HttpResponseCodes.Unauthorized;
                return;
            }
        }

        await next.InvokeAsync(context, cancellationToken);
    }
}
