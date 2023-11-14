using Microsoft.IdentityModel.Tokens;

namespace MiniWebServer.Authentication
{
    public class JwtAuthenticationOptions
    {
        public JwtAuthenticationOptions(TokenValidationParameters tokenValidationParameters)
        {
            TokenValidationParameters = tokenValidationParameters ?? throw new ArgumentNullException(nameof(tokenValidationParameters));
        }

        public TokenValidationParameters TokenValidationParameters { get; }
    }
}
