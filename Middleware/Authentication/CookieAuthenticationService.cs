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
        private CookieAuthenticationOptions options;
        private readonly ILogger<CookieAuthenticationService> logger;

        public CookieAuthenticationService(CookieAuthenticationOptions? options, ILoggerFactory? loggerFactory) { 
            this.options = options ?? new CookieAuthenticationOptions();

            if (loggerFactory != null)
                logger = loggerFactory.CreateLogger<CookieAuthenticationService>();
            else
                logger = NullLogger<CookieAuthenticationService>.Instance;
        }
        public async Task<AuthenticationResult> AuthenticateAsync(IMiniAppContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            logger.LogDebug("Authenticating request using cookie...");

            if (context.Request.Cookies != null)
            {
                if (context.Request.Cookies.TryGetValue(options.CookieName, out HttpCookie? cookie))
                {
                    if (cookie != null)
                    {
                        if (await IsValidAuthenticationCookie(cookie.Value))
                        {
                            return new AuthenticationResult(false, null);
                        }
                    }
                }
            }

            return new AuthenticationResult(false, null);
        }

        private Task<bool> IsValidAuthenticationCookie(string value)
        {
            // todo: validate authentication cookie
            return Task.FromResult(false);
        }
    }
}
