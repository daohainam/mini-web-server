using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Security
{
    public class ClaimsPrincipal: IPrincipal
    {
        public ClaimsPrincipal() { 
            Claims = new List<Claim>();
        }
        public ClaimsPrincipal(IIdentity? identity, IEnumerable<Claim> claims)
        {
            Identity = identity;
            Claims = claims ?? throw new ArgumentNullException(nameof(claims));
        }

        public IIdentity? Identity { get; }
        public IEnumerable<Claim> Claims { get; }

        public bool IsInRole(string role)
        {
            return Claims.Where(c => c.Type == ClaimTypes.Role && c.Value == role).Any();
        }
    }
}
