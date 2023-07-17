using MiniWebServer.MiniApp.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authentication
{
    public interface IPrincipalStore
    {
        ClaimsPrincipal? GetPrincipal(string key);
        bool SetPrincipal(string key, ClaimsPrincipal principal);
    }
}
