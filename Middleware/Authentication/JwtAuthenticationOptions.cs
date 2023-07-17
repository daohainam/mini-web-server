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
        public JwtAuthenticationOptions(TokenValidationParameters tokenValidationParameters)
        {
            TokenValidationParameters = tokenValidationParameters ?? throw new ArgumentNullException(nameof(tokenValidationParameters));
        }

        public TokenValidationParameters TokenValidationParameters { get; }
    }
}
