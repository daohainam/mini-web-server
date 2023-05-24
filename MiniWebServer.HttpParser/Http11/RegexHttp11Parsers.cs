using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Abstractions.HttpParser.Http11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MiniWebServer.HttpParser.Http11
{
    /// <summary>
    /// this class uses samples from: https://stackoverflow.com/questions/27457949/check-pattern-of-http-get-using-regexc
    /// we will need a ABNF-based parser 
    /// </summary>
    public partial class RegexHttp11Parsers : IHttp11Parser
    {
        public Http11RequestLine? ParseRequestLine(string text)
        {
            var httpRegex = HttpRequestLineRegex();
            var match = httpRegex.Match(text);
            if (match.Success)
            {
                var paramNameGroup = match.Groups["paramName"];
                var paramValueGroup = match.Groups["paramValue"];

                HttpParameters httpParameters = new();
                for (int i = 0; i < paramNameGroup.Captures.Count; i++)
                {
                    httpParameters.Add(new HttpParameter(paramNameGroup.Captures[i].Value, paramValueGroup.Captures[i].Value));
                }

                return new Http11RequestLine(
                    match.Groups["method"].Value,
                    match.Groups["url"].Value,
                    match.Groups["hash"].Value,
                    match.Groups["queryString"].Value,
                    new Http11ProtocolVersion(match.Groups["major"].Value, match.Groups["minor"].Value),
                    httpParameters
                    );
            }

            return null;
        }

        public Http11HeaderLine? ParseHeaderLine(string text)
        {
            var httpRegex = HttpHeaderLineRegex();
            var match = httpRegex.Match(text);
            if (match.Success)
            {
                return new Http11HeaderLine(
                    match.Groups["name"].Value,
                    match.Groups["value"].Value
                    );
            }

            return null;
        }

        public IEnumerable<HttpCookie>? ParseCookieHeader(string text)
        {
            var httpRegex = HttpCookieValueRegex();
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

        // https://regex101.com/r/QLes5d/1
        [GeneratedRegex("^(?<method>[a-zA-Z]+)\\s(?<url>/[^\\r\\n\\?]*)(?<queryString>\\?((?<params>(?<paramName>\\w+)+=(?<paramValue>[\\w|%]*))&?)*)?(?<hash>#\\w*)?\\sHTTP/(?<major>\\d)\\.(?<minor>\\d+)$")]
        private static partial Regex HttpRequestLineRegex();

        [GeneratedRegex(@"(?<name>[\w-_]+): ?(?<value>[\w\-  _ :;.,\\/'?!(){}\[\]@<>=\-+\*#$&`|~^%""]+) ?")]
        private static partial Regex HttpHeaderLineRegex();

        [GeneratedRegex(@"^(((?<cookieName>[\w-_\.]+)=(?<cookieValue>[\w\-  _ :.,\\/'?!(){}\[\]@<>=\-+\*#$&`|~^%""]*))(; )?)*$")]
        private static partial Regex HttpCookieValueRegex();

    }
}
