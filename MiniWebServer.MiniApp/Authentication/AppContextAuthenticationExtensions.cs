using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Authentication
{
    public static class AppContextAuthenticationExtensions
    {
        public static void SignIn(this IMiniAppContext context, ClaimsPrincipal principal)
        {
            context.User = principal;
        }

        public static void SignOut(this IMiniAppContext context)
        {
            context.User = null;
        }
    }
}
