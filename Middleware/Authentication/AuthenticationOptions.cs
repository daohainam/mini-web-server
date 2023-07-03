using MiniWebServer.MiniApp.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authentication
{
    public class AuthenticationOptions
    {
        public JwtAuthenticationOptions? JwtAuthenticationOptions { get; init; }
    }
}
