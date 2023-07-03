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
            IsSucceeded = succeeded;
            Principal = principal;
        }

        public bool IsSucceeded { get; }
        public IPrincipal? Principal { get; }

        public static readonly AuthenticationResult Failed = new(false, null);
    }
}
