using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.IO;

namespace MiniWebServer.Authentication
{
    public class JwtAuthenticationService : IAuthenticationService
    {
        private readonly ILogger<JwtAuthenticationService> logger;
        private readonly JwtAuthenticationOptions? options;

        public JwtAuthenticationService(JwtAuthenticationOptions? options, ILoggerFactory? loggerFactory)
        {
            this.options = options ?? new JwtAuthenticationOptions();

            if (loggerFactory != null)
                logger = loggerFactory.CreateLogger<JwtAuthenticationService>();
            else
                logger = NullLogger<JwtAuthenticationService>.Instance;
        }

        public Task<AuthenticationResult> AuthenticateAsync(IMiniAppContext context)
        {
            var authHeader = context.Request.Headers.Authorization;

            if (!string.IsNullOrEmpty(authHeader))
            {
                if (authHeader.StartsWith("Bearer "))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadToken(authHeader[7..]) as JwtSecurityToken;

                    if (token != null)
                    {
                        // todo: validate token and set Request.User
                    }
                }
            }

            return Task.FromResult(AuthenticationResult.Failed);
        }
    }
}
