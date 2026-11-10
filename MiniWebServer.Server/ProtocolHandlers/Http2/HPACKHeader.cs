namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// A decoded HPACK header field (RFC 7541). Names are kept exactly as decoded, HTTP/2 requires them to be lowercase.
/// </summary>
public sealed class HPACKHeader(string name, string value, bool sensitive = false)
{
    // RFC 7541 4.1: the size of an entry is the sum of its name length, its value length and 32 octets
    public const int EntryOverhead = 32;

    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public string Value { get; } = value ?? throw new ArgumentNullException(nameof(value));

    /// <summary>
    /// True when the field was received as "literal never indexed", intermediaries must not compress it.
    /// </summary>
    public bool Sensitive { get; } = sensitive;

    public int Size => EntryOverhead + Name.Length + Value.Length;

    public override string ToString() => $"{Name}: {Value}";
}
