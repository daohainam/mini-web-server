using System.Security.Claims;

namespace MiniWebServer.MiniApp.Authentication;

public class AuthenticationResult(bool succeeded, ClaimsPrincipal? principal)
{
    public bool IsSucceeded { get; } = succeeded;
    public ClaimsPrincipal? Principal { get; } = principal;

    public static readonly AuthenticationResult Failed = new(false, null);
}
