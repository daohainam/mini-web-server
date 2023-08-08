using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authentication
{
    public interface IPrincipalStore
    {
        ClaimsPrincipal? GetPrincipal(string key);
        bool RemovePrincipal(string key);
        bool SetPrincipal(string key, ClaimsPrincipal principal);
    }
}
