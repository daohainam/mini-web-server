using MiniWebServer.Abstractions.HttpParser.Http11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniWebServer.HttpParser.Http11
{
    /// <summary>
    /// this class uses samples from: https://stackoverflow.com/questions/27457949/check-pattern-of-http-get-using-regexc
    /// </summary>
    public partial class RegexHttp11Parsers : IHttp11Parser
    {
        public Http11RequestLine? ParseRequestLine(string text)
        {
            var httpRegex = HttpRequestLineRegex();
            var match = httpRegex.Match(text);
            if (match.Success)
            {
                return new Http11RequestLine(
                    match.Groups["method"].Value,
                    match.Groups["url"].Value,
                    new Http11ProtocolVersion(match.Groups["major"].Value, match.Groups["minor"].Value)
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

        [GeneratedRegex("^(?<method>[a-zA-Z]+)\\s(?<url>.+)\\sHTTP/(?<major>\\d)\\.(?<minor>\\d+)$")]
        private static partial Regex HttpRequestLineRegex();

        [GeneratedRegex(@"(?<name>[\w-_]+): ?(?<value>[\w\-  _ :;.,\\/'?!(){}\[\]@<>=\-+\*#$&`|~^%""]+) ?")]
        private static partial Regex HttpHeaderLineRegex();
    }
}
