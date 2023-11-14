using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Abstractions.Parsers;
using System.Text.RegularExpressions;

namespace MiniWebServer.Server.Cookie
{
    /// <summary>
    /// Parse a cookie value sent by client (https://datatracker.ietf.org/doc/html/rfc6265)
    /// </summary>
    public partial class DefaultCookieParser : ICookieValueParser
    {
        private readonly ILoggerFactory loggerFactory;

        public DefaultCookieParser(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public virtual IEnumerable<HttpCookie>? ParseCookieHeader(string text)
        {
            var httpRegex = HttpCookieValueRegex(); // todo: I don't like using RegEx since it is sometimes slow and error-prone
            var match = httpRegex.Match(text);
            if (match.Success)
            {
                var paramNameGroup = match.Groups["cookieName"];
                var paramValueGroup = match.Groups["cookieValue"];

                var cookies = new List<HttpCookie>();
                for (int i = 0; i < paramNameGroup.Captures.Count; i++)
                {
                    cookies.Add(new HttpCookie(paramNameGroup.Captures[i].Value, paramValueGroup.Captures[i].Value));
                }

                return cookies;
            }

            return null;
        }

        [GeneratedRegex(@"^(((?<cookieName>[\w-_\.]+)=(?<cookieValue>[\w\-  _ :.,\\/'?!(){}\[\]@<>=\-+\*#$&`|~^%""]*))(; )?)*$")]
        private static partial Regex HttpCookieValueRegex();

    }
}
