using MiniWebServer.MiniApp.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Authentication
{
    public class AuthenticationResult
    {
        public AuthenticationResult(bool succeeded, IPrincipal? principal)
        {
            Succeeded = succeeded;
            Principal = principal;
        }

        public bool Succeeded { get; }
        public IPrincipal? Principal { get; }
    }
}
