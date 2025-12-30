namespace MiniWebServer.MiniApp.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(IMiniAppRequestContext context);
    Task SignInAsync(IMiniAppRequestContext context, System.Security.Claims.ClaimsPrincipal principal);
    Task SignOutAsync(IMiniAppRequestContext context);
}
