namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// The payload of a PRIORITY frame (RFC 9113 6.3). The priority scheme is deprecated, the fields are only validated.
/// </summary>
public sealed record Http2FramePRIORITYPayload
{
    public bool Exclusive { get; init; }
    public uint StreamDependency { get; init; }
    public byte Weight { get; init; }

    public static readonly Http2FramePRIORITYPayload Empty = new();
}
