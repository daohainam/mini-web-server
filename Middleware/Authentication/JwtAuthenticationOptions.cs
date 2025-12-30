using Microsoft.IdentityModel.Tokens;

namespace MiniWebServer.Authentication;

public class JwtAuthenticationOptions(TokenValidationParameters tokenValidationParameters)
{
    public TokenValidationParameters TokenValidationParameters { get; } = tokenValidationParameters ?? throw new ArgumentNullException(nameof(tokenValidationParameters));
}
