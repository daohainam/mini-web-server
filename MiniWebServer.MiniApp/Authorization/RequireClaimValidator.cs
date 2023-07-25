using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Authorization
{
    internal class RequireClaimValidator : IClaimValidator
    {
        private readonly string requiredClaim;

        public RequireClaimValidator(string requiredClaim)
        {
            this.requiredClaim = requiredClaim;
        }

        public bool Validate(IMiniAppContext context)
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
