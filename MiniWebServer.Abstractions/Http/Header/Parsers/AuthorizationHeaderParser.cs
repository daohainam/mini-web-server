using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http.Header.Parsers
{
    internal class AuthorizationHeaderParser : IHeaderParser
    {
        public object? Parse(string? value)
        {
            if (value == null) 
                return null;

            int idx = value.IndexOf(' ');
            if (idx < 0 )
            {
                return new AuthorizationHeader()
                {
                    Scheme = value
                };
            }
            else
            {
                return new AuthorizationHeader()
                {
                    Scheme = value[..idx],
                    Parameters = value[(idx + 1)..]
                };
            }
        }
    }
}
