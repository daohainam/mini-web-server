using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class HPACKHeader(int staticTableIndex, string name, string value)
    {
        public int StaticTableIndex { get; } = staticTableIndex;
        public string Name { get; } = name;
        public string Value { get; } = value;
        public byte[] NameAsBytes { get; } = name.Length > 0 ? Encoding.ASCII.GetBytes(name) : [];
        public byte[] ValueAsBytes { get; } = value.Length > 0 ? Encoding.ASCII.GetBytes(value) : [];
    }
}
