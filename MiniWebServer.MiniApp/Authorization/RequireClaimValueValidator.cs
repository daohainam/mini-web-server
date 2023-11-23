namespace MiniWebServer.MiniApp.Authorization
{
    internal class RequireClaimValueValidator : IClaimValidator
    {
        private readonly string requiredClaim;
        private readonly string value;
        private readonly StringComparison stringComparison;

        public RequireClaimValueValidator(string requiredClaim, string value, StringComparison? stringComparison = default)
        {
            this.requiredClaim = requiredClaim;
            this.value = value;
            this.stringComparison = stringComparison ?? StringComparison.InvariantCulture;
        }

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
