namespace MiniWebServer.MiniApp.Authentication
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(IMiniAppContext context);
        Task SignInAsync(IMiniAppContext context, System.Security.Claims.ClaimsPrincipal principal);
        Task SignOutAsync(IMiniAppContext context);
    }
}
