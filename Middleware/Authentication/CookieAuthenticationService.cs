using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authentication;

namespace MiniWebServer.Authentication
{
    public class CookieAuthenticationService : IAuthenticationService
    {
        private readonly CookieAuthenticationOptions options;
        private readonly IPrincipalKeyGenerator principalKeyGenerator;
        private readonly ILogger<CookieAuthenticationService> logger;

        public CookieAuthenticationService(CookieAuthenticationOptions? options, IPrincipalKeyGenerator? principalKeyGenerator, ILoggerFactory? loggerFactory)
        {
            this.options = options ?? new CookieAuthenticationOptions();
            this.principalKeyGenerator = principalKeyGenerator ?? new DefaultPrincipalKeyGenerator();

            if (loggerFactory != null)
                logger = loggerFactory.CreateLogger<CookieAuthenticationService>();
            else
                logger = NullLogger<CookieAuthenticationService>.Instance;
        }
        public async Task<AuthenticationResult> AuthenticateAsync(IMiniAppRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            try
            {
                logger.LogDebug("Authenticating request using cookie...");

                if (context.Request.Cookies != null)
                {
                    var principalStore = context.Services.GetService<IPrincipalStore>();

                    if (principalStore == null)
                    {
                        logger.LogWarning("No IPrincipleStore registered");
                    }
                    else
                    {
                        if (context.Request.Cookies.TryGetValue(options.CookieName, out HttpCookie? cookie))
                        {
                            if (cookie != null)
                            {
                                if (!(await IsValidAuthenticationCookieAsync(cookie.Value)))
                                {
                                    return new AuthenticationResult(false, null);
                                }

                                var principle = principalStore.GetPrincipal(cookie.Value);
                                if (principle != null)
                                {
                                    return new AuthenticationResult(true, principle);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error authenticating");
            }

            return new AuthenticationResult(false, null);
        }

        public Task SignInAsync(IMiniAppRequestContext context, System.Security.Claims.ClaimsPrincipal principal)
        {
            var principalStore = context.Services.GetService<IPrincipalStore>();

            if (principalStore == null)
            {
                logger.LogWarning("No IPrincipleStore registered");
            }
            else
            {
                string? key = principalKeyGenerator.GeneratePrincipalKey(principal);

                if (!string.IsNullOrEmpty(key))
                {
                    if (principalStore.SetPrincipal(key, principal))
                    {
                        context.Response.Cookies.TryAdd(options.CookieName, new HttpCookie(
                            options.CookieName,
                            key,
                            httpOnly: true
                        )
                        );
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task SignOutAsync(IMiniAppRequestContext context)
        {
            var principal = context.User;
            if (principal == null)
                return Task.CompletedTask;

            context.Response.Cookies.Remove(options.CookieName);
            var principalStore = context.Services.GetService<IPrincipalStore>();

            if (principalStore == null)
            {
                logger.LogWarning("No IPrincipleStore registered");
            }
            else
            {
                string? key = principalKeyGenerator.GeneratePrincipalKey(principal);

                if (!string.IsNullOrEmpty(key))
                {
                    principalStore.RemovePrincipal(key);
                }
            }
            return Task.CompletedTask;
        }

        protected Task<bool> IsValidAuthenticationCookieAsync(string cookieValue)
        {
            // todo: validate authentication cookie
            return Task.FromResult(false);
        }
    }
}
