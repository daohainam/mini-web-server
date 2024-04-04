using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public class HPACKHeader(HPACKHeaderTypes headerType, int staticTableIndex, string name, string value)
    {
        public HPACKHeader(string name, string value) : this(HPACKHeaderTypes.Literal, 0, name, value)
        {
        }
        public HPACKHeader(HPACKHeader header) : this(header.HeaderType, header.StaticTableIndex, header.Name, header.Value)
        {
        }

        public int StaticTableIndex { get; } = staticTableIndex;
        public string Name { get; } = name;
        public string Value { get; } = value;
        //public byte[] NameAsBytes { get; } = name.Length > 0 ? Encoding.ASCII.GetBytes(name) : [];
        //public byte[] ValueAsBytes { get; } = value.Length > 0 ? Encoding.ASCII.GetBytes(value) : [];
        public HPACKHeaderTypes HeaderType { get; } = headerType;
    }

    public enum HPACKHeaderTypes
    {
        Static,
        Literal
    }
}
