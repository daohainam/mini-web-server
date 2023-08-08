using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authentication
{
    public interface IPrincipalKeyGenerator
    {
        string? GeneratePrincipalKey(IPrincipal principal);
    }
}
