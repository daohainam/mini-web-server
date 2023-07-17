using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer
{
    internal class DemoAppConfig
    {
        public JwtConfig? Jwt { get; set; }
    }

    internal class JwtConfig
    {
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? SecretKey { get; set; }
    }
}
