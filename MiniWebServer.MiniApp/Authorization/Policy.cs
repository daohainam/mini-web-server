namespace MiniWebServer.MiniApp.Authorization;

public class Policy : IPolicy
{
    private readonly List<IClaimValidator> claims = [];

    public IPolicy RequireClaim(string claim)
    {
        claims.Add(
            new RequireClaimValidator(claim)
            );

        return this;
    }

    public IPolicy RequireClaimValue(string claim, string value, StringComparison? stringComparison = default)
    {
        claims.Add(
            new RequireClaimValueValidator(claim, value, stringComparison)
            );

        return this;
    }

    public bool IsValid(IMiniAppRequestContext context)
    {
        foreach (var claim in claims)
        {
            if (!claim.Validate(context))
                return false;
        }

        return true;
    }
}
