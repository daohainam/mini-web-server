using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Authorization
{
    public interface IAuthorizationPolicy
    {
        bool IsAuthorized(IPrincipal principal);
    }
}
