using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public class Http2Exception : Exception
{
    public Http2Exception()
    {
    }

    public Http2Exception(string? message) : base(message)
    {
    }

    public Http2Exception(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
