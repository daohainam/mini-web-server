using System.Text.RegularExpressions;

namespace MiniWebServer.Abstractions.Http.Header.Parsers
{
    internal partial class RangeHeaderParser : IHeaderParser
    {
        public static bool TryParse(string s, out RangeHeader? rangeHeader)
        {
            // https://datatracker.ietf.org/doc/html/rfc7233
            rangeHeader = null;

            var httpRegex = RangeHeaderRegex();
            var match = httpRegex.Match(s);
            if (match.Success)
            {
                if (match.Groups["unit"].Value != "bytes")
                    return false;

                List<RangePart> parts = [];

                var startGroup = match.Groups["start"];
                var endGroup = match.Groups["end"];
                for (int i = 0; i < startGroup.Captures.Count; i++)
                {
                    if (!long.TryParse(startGroup.Captures[i].Value, out long start))
                        return false;

                    long end = long.MaxValue;
                    if (!string.Empty.Equals(endGroup.Captures[i].Value))
                    {
                        if (!long.TryParse(endGroup.Captures[i].Value, out end))
                            return false;
                    }

                    if (start > end)
                    {
                        return false;
                    }

                    parts.Add(new RangePart(start, end));
                }

                rangeHeader = new RangeHeader(
                    RangeUnits.Bytes,
                    [.. parts]
                    );
                return true;
            }

            return false;
        }

        [GeneratedRegex(@"(?<unit>[\w-_]+)=(?<values>((?<start>[\d]+)-(?<end>[\d]*))(, )?)+")]
        private static partial Regex RangeHeaderRegex();

        public object? Parse(string? value)
        {
            if (value == null)
                return null;

            if (TryParse(value, out var rangeHeader))
            {
                return rangeHeader;
            }
            else
            {
                return null;
            }
        }
    }
}
