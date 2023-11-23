namespace MiniWebServer.MiniApp.Authorization
{
    public interface IPolicy
    {
        bool IsValid(IMiniAppRequestContext context);
        IPolicy RequireClaim(string claim);
        IPolicy RequireClaimValue(string claim, string value, StringComparison? stringComparison = default);
    }
}