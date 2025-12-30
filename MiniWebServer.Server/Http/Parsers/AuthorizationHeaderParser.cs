using MiniWebServer.Abstractions.Http.Header;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Http.Parsers;

internal class AuthorizationHeaderParser
{
    public static bool TryParse(IEnumerable<string> value, out AuthorizationHeader? authorization)
    {
        return TryParse(value.FirstOrDefault(), out authorization);
    }
    public static bool TryParse(string? value, out AuthorizationHeader? authorization)
    {
        if (value == null)
        {
            authorization = null;
            return false;
        }

        int idx = value.IndexOf(' ');
        if (idx < 0)
        {
            authorization = new AuthorizationHeader()
            {
                Scheme = value
            };
            return true;
        }
        else
        {
            authorization = new AuthorizationHeader()
            {
                Scheme = value[..idx],
                Parameters = value[(idx + 1)..]
            };
            return true;
        }
    }
}
