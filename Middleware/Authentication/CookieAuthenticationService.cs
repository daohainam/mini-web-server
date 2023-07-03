using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authentication
{
    public class CookieAuthenticationService : IAuthenticationService
    {
        private readonly CookieAuthenticationOptions options;
        private readonly ILogger<CookieAuthenticationService> logger;

        public CookieAuthenticationService(CookieAuthenticationOptions? options, ILoggerFactory? loggerFactory)
        {
            this.options = options ?? new CookieAuthenticationOptions();

            if (loggerFactory != null)
                logger = loggerFactory.CreateLogger<CookieAuthenticationService>();
            else
                logger = NullLogger<CookieAuthenticationService>.Instance;
        }
        public async Task<AuthenticationResult> AuthenticateAsync(IMiniAppContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            try
            {
                logger.LogDebug("Authenticating request using cookie...");

                if (context.Request.Cookies != null)
                {
                    var princpalStore = context.Services.GetService<IPrincipalStore>();

                    if (princpalStore == null)
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

                                var principle = princpalStore.GetPrincipal(cookie.Value);
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

        private Task<bool> IsValidAuthenticationCookieAsync(string cookieValue)
        {
            // todo: validate authentication cookie
            return Task.FromResult(false);
        }
    }
}
