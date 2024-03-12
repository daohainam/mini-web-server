using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class HPACKHeader(int staticTableIndex, byte[] name, byte[] value)
    {
        public int StaticTableIndex { get; } = staticTableIndex;
        public byte[] Name { get; } = name;
        public byte[] Value { get; } = value;
    }
}
