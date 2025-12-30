namespace MiniWebServer.Authentication;

public class AuthenticationOptions
{
    public string? DefaultAuthenticateScheme { get; set; }
    public JwtAuthenticationOptions? JwtAuthenticationOptions { get; init; }
}
