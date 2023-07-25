using MiniWebServer.MiniApp;

namespace MiniWebServer.MiniApp.Authorization
{
    public interface IPolicy
    {
        bool IsValid(IMiniAppContext context);
        IPolicy RequireClaim(string claim);
        IPolicy RequireClaimValue(string claim, string value, StringComparison? stringComparison = default);
    }
}