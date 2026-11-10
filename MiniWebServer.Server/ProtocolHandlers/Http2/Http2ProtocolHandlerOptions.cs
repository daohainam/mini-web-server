namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// The settings this server advertises to its peers and the limits it enforces on incoming frames.
/// </summary>
public class Http2ProtocolHandlerOptions
{
    public const uint DefaultMaxFrameSize = 16_384; // RFC 9113 4.2: 2^14
    public const uint DefaultInitialWindowSize = 65_535; // RFC 9113 6.9.2: 2^16 - 1
    public const uint MaxWindowSize = 0x7FFF_FFFF; // RFC 9113 6.9.1: 2^31 - 1
    public const uint MinMaxFrameSize = 16_384;
    public const uint MaxMaxFrameSize = 16_777_215; // RFC 9113 6.5.2: 2^24 - 1

    /// <summary>
    /// SETTINGS_MAX_CONCURRENT_STREAMS: how many client streams may be open at once.
    /// </summary>
    public uint MaxConcurrentStreams { get; set; } = 100;

    /// <summary>
    /// SETTINGS_INITIAL_WINDOW_SIZE: the receive window granted to each new stream.
    /// </summary>
    public uint InitialWindowSize { get; set; } = DefaultInitialWindowSize;

    /// <summary>
    /// SETTINGS_MAX_FRAME_SIZE: the largest frame payload we accept.
    /// </summary>
    public uint MaxFrameSize { get; set; } = DefaultMaxFrameSize;

    /// <summary>
    /// SETTINGS_HEADER_TABLE_SIZE: the largest dynamic table the peer's encoder may use.
    /// </summary>
    public uint HeaderTableSize { get; set; } = HPACKHeaderTable.DefaultMaxSize;

    /// <summary>
    /// SETTINGS_MAX_HEADER_LIST_SIZE: the largest decoded header list we accept, measured as in RFC 9113 6.5.2.
    /// </summary>
    public uint MaxHeaderListSize { get; set; } = HPACKDecoder.DefaultMaxHeaderListSize;

    public int MaxHeaderCount { get; set; } = HPACKDecoder.DefaultMaxHeaderCount;

    /// <summary>
    /// The largest header block (all HEADERS and CONTINUATION fragments together) buffered before decoding.
    /// </summary>
    public int MaxHeaderBlockSize { get; set; } = 128 * 1024;
}
