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
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlTypes;

namespace MiniWebServer.Authentication
{
    public class JwtAuthenticationService : IAuthenticationService
    {
        private readonly ILogger<JwtAuthenticationService> logger;
        private readonly JwtAuthenticationOptions options;

        public JwtAuthenticationService(JwtAuthenticationOptions? options, ILoggerFactory? loggerFactory)
        {
            this.options = options ?? new JwtAuthenticationOptions();

            if (loggerFactory != null)
                logger = loggerFactory.CreateLogger<JwtAuthenticationService>();
            else
                logger = NullLogger<JwtAuthenticationService>.Instance;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(IMiniAppContext context)
        {
            try
            {
                var authHeader = context.Request.Headers.Authorization;

                if (!string.IsNullOrEmpty(authHeader))
                {
                    if (authHeader.StartsWith("Bearer "))
                    {
                        logger.LogInformation("Validating JWT token...");

                        var handler = new JwtSecurityTokenHandler();
                        var token = handler.ReadToken(authHeader[7..]) as JwtSecurityToken;

                        if (await ValidateAsync(token, handler, options.TokenValidationParameters))
                        {
                            // todo: validate token and set Request.User
                            logger.LogInformation("Token validated");
                        }
                        else
                        {
                            logger.LogInformation("Token not valid"); // note: this not an app error so we don't use LogError
                        }
                    }
                }
            } catch (Exception ex)
            {
                logger.LogError(ex, "Error authenticating");
            }

            return AuthenticationResult.Failed;
        }

        private async Task<bool> ValidateAsync(JwtSecurityToken? token, JwtSecurityTokenHandler handler, TokenValidationParameters? tokenValidationParameters)
        {
            if (tokenValidationParameters == null)
                return false;

            var result = await handler.ValidateTokenAsync(token, tokenValidationParameters);
            return result.IsValid;
        }
    }
}
