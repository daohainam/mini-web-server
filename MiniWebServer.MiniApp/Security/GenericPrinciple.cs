using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Security
{
    public class GenericPrinciple : IPrincipal
    {
        public GenericPrinciple(IIdentity? identity, string[] roles)
        {
            Identity = identity;
            Roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        public IIdentity? Identity { get; }
        public string[] Roles { get; }

        public bool IsInRole(string role)
        {
            return Roles.Contains(role);
        }
    }
}
