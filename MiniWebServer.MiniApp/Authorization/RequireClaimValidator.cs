namespace MiniWebServer.MiniApp.Authorization
{
    internal class RequireClaimValidator : IClaimValidator
    {
        private readonly string requiredClaim;

        public RequireClaimValidator(string requiredClaim)
        {
            this.requiredClaim = requiredClaim;
        }

        public bool Validate(IMiniAppRequestContext context)
        {
            if (context == null
                || context.User == null
                || !context.User.Claims.Where(c => c.Type == requiredClaim).Any()
                )
            {
                return false;
            }

            return true;
        }
    }
}
