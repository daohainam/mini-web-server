using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Authentication
{
    public class JwtAuthenticationOptions
    {
        public TokenValidationParameters? TokenValidationParameters { get; set; }
    }
}
