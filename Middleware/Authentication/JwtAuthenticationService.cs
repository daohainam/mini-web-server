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
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.IdentityModel.Logging;

namespace MiniWebServer.Authentication
{
    public class JwtAuthenticationService : IAuthenticationService
    {
        private readonly ILogger<JwtAuthenticationService> logger;
        private readonly JwtAuthenticationOptions options;

        public JwtAuthenticationService(JwtAuthenticationOptions options, ILoggerFactory? loggerFactory)
        {
            this.options = options;

            if (loggerFactory != null)
                logger = loggerFactory.CreateLogger<JwtAuthenticationService>();
            else
                logger = NullLogger<JwtAuthenticationService>.Instance;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(IMiniAppContext context)
        {
            try
            {
                IdentityModelEventSource.ShowPII = true;
                var authHeader = context.Request.Headers.Authorization;

                if (!string.IsNullOrEmpty(authHeader))
                {
                    if (authHeader.StartsWith("Bearer "))
                    {
                        logger.LogInformation("Validating JWT token...");

                        var handler = new JwtSecurityTokenHandler();
                        var result = await ValidateAsync(authHeader[7..], handler, options.TokenValidationParameters);
                        if (result.IsValid)
                        {
                            logger.LogInformation("Token validated");

                            var roles = new List<string>();
                            foreach (var claim in result.Claims)
                            {
                                if (ClaimTypes.Role.Equals(claim.Key))
                                {
                                    if (claim.Value is List<object> values)
                                    {
                                        foreach (string role in values.Cast<string>())
                                        {
                                            roles.Add(role);
                                        }
                                    }
                                }
                            }

                            context.User = new GenericPrincipal(result.ClaimsIdentity, roles.ToArray());
                            //context.User = new GenericPrincipal(result.ClaimsIdentity, Array.Empty<string>());

                            return new AuthenticationResult(true, context.User);
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

        public Task SignInAsync(IMiniAppContext context, ClaimsPrincipal principal)
        {
            return Task.CompletedTask;
        }

        public Task SignOutAsync(IMiniAppContext context)
        {
            return Task.CompletedTask;
        }

        private async Task<TokenValidationResult> ValidateAsync(string token, JwtSecurityTokenHandler handler, TokenValidationParameters? tokenValidationParameters/*, out GenericPrinciple? principle*/)
        {
            //principle = null;

            if (tokenValidationParameters == null)
                return new TokenValidationResult() { IsValid = false };

            
            var result = await handler.ValidateTokenAsync(token, tokenValidationParameters);

            if (!result.IsValid)
            {
                logger.LogInformation("Token rejected: {m}, exeption: {ex}", result, result.Exception);
            }

            return result;
        }
    }
}
