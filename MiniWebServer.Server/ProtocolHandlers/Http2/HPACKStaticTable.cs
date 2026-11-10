namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// The HPACK static table, RFC 7541 Appendix A.
/// </summary>
internal static class HPACKStaticTable
{
    public const string AUTHORITY = ":authority";
    public const string METHOD = ":method";
    public const string PATH = ":path";
    public const string SCHEME = ":scheme";
    public const string STATUS = ":status";

    public const int TableSize = 61;

    public static readonly HPACKHeader[] Entries =
    [
        new(AUTHORITY, ""),
        new(METHOD, "GET"),
        new(METHOD, "POST"),
        new(PATH, "/"),
        new(PATH, "/index.html"),
        new(SCHEME, "http"),
        new(SCHEME, "https"),
        new(STATUS, "200"),
        new(STATUS, "204"),
        new(STATUS, "206"),
        new(STATUS, "304"),
        new(STATUS, "400"),
        new(STATUS, "404"),
        new(STATUS, "500"),
        new("accept-charset", ""),
        new("accept-encoding", "gzip, deflate"),
        new("accept-language", ""),
        new("accept-ranges", ""),
        new("accept", ""),
        new("access-control-allow-origin", ""),
        new("age", ""),
        new("allow", ""),
        new("authorization", ""),
        new("cache-control", ""),
        new("content-disposition", ""),
        new("content-encoding", ""),
        new("content-language", ""),
        new("content-length", ""),
        new("content-location", ""),
        new("content-range", ""),
        new("content-type", ""),
        new("cookie", ""),
        new("date", ""),
        new("etag", ""),
        new("expect", ""),
        new("expires", ""),
        new("from", ""),
        new("host", ""),
        new("if-match", ""),
        new("if-modified-since", ""),
        new("if-none-match", ""),
        new("if-range", ""),
        new("if-unmodified-since", ""),
        new("last-modified", ""),
        new("link", ""),
        new("location", ""),
        new("max-forwards", ""),
        new("proxy-authenticate", ""),
        new("proxy-authorization", ""),
        new("range", ""),
        new("referer", ""),
        new("refresh", ""),
        new("retry-after", ""),
        new("server", ""),
        new("set-cookie", ""),
        new("strict-transport-security", ""),
        new("transfer-encoding", ""),
        new("user-agent", ""),
        new("vary", ""),
        new("via", ""),
        new("www-authenticate", "")
    ];

    // first static index for a name, and the index of each name/value pair that has a non-empty value
    private static readonly Dictionary<string, int> nameIndexes = BuildNameIndexes();
    private static readonly Dictionary<(string Name, string Value), int> pairIndexes = BuildPairIndexes();

    private static Dictionary<string, int> BuildNameIndexes()
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < Entries.Length; i++)
        {
            map.TryAdd(Entries[i].Name, i + 1);
        }

        return map;
    }

    private static Dictionary<(string Name, string Value), int> BuildPairIndexes()
    {
        var map = new Dictionary<(string Name, string Value), int>();
        for (int i = 0; i < Entries.Length; i++)
        {
            if (Entries[i].Value.Length > 0)
            {
                map.TryAdd((Entries[i].Name, Entries[i].Value), i + 1);
            }
        }

        return map;
    }

    /// <summary>
    /// Returns the entry at the given 1-based static index, or null when the index is out of range.
    /// </summary>
    public static HPACKHeader? GetHeader(int staticTableIndex)
    {
        if (staticTableIndex >= 1 && staticTableIndex <= Entries.Length)
        {
            return Entries[staticTableIndex - 1];
        }

        return null;
    }

    public static int GetNameIndex(string name) => nameIndexes.TryGetValue(name, out var index) ? index : 0;

    public static int GetIndex(string name, string value) => pairIndexes.TryGetValue((name, value), out var index) ? index : 0;
}
