using System.Buffers;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

/// <summary>
/// The fields of a HEADERS frame payload (RFC 9113 6.2) with padding and the optional priority fields stripped off.
/// </summary>
public sealed class Http2FrameHEADERSPayload
{
    public byte PadLength { get; init; }
    public bool HasPriority { get; init; }
    public bool Exclusive { get; init; }
    public uint StreamDependency { get; init; }
    public byte Weight { get; init; }

    /// <summary>
    /// The HPACK header block fragment carried by this frame. It is only a fragment, CONTINUATION frames may follow.
    /// </summary>
    public ReadOnlySequence<byte> FieldBlockFragment { get; init; }
}
