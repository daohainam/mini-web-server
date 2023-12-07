namespace MiniWebServer.MiniApp.Authorization
{
    internal class RequireClaimValidator(string requiredClaim) : IClaimValidator
    {
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
