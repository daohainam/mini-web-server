using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Authorization
{
    public interface IAuthorizationPolicyBuilder
    {
        IAuthorizationPolicyBuilder RequireRole(string role);

        IAuthorizationPolicy Build();
    }
}
