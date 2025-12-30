using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public record Http2FramePRIOTITYPayload
{
    public bool Exclusive { get; init; }
    public uint StreamDependency { get; init; }
    public byte Weight { get; init; }

    public static readonly Http2FramePRIOTITYPayload Empty = new();
}
