using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Abstractions.Parsers
{
    public interface ICookieValueParser
    {
        IEnumerable<HttpCookie>? ParseCookieHeader(string value);

    }
}
