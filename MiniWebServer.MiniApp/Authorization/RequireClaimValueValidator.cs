namespace MiniWebServer.MiniApp.Authorization
{
    internal class RequireClaimValueValidator(string requiredClaim, string value, StringComparison? stringComparison = default) : IClaimValidator
    {
        private readonly StringComparison stringComparison = stringComparison ?? StringComparison.InvariantCulture;

        public bool Validate(IMiniAppRequestContext context)
        {
            if (context == null
                || context.User == null
                )
            {
                return false;
            }

            return context.User.Claims.Where(c => c.Type == requiredClaim && value.Equals(c.Value, stringComparison)).Any();
        }
    }
}
