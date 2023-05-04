using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.HttpParser.Http11
{
    public class Http11ProtocolVersion
    {
        public Http11ProtocolVersion(string major, string minor)
        {
            Major = major ?? throw new ArgumentNullException(nameof(major));
            Minor = minor ?? throw new ArgumentNullException(nameof(minor));
        }

        public string Major { get; }
        public string Minor { get; }
    }
    public class Http11RequestLine
    {
        public Http11RequestLine(string method, string url, Http11ProtocolVersion protocolVersion)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            ProtocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
        }

        public string Method { get; }
        public string Url { get; }
        public Http11ProtocolVersion ProtocolVersion { get; }
    }

    public class Http11HeaderLine
    {
        public Http11HeaderLine(string name, string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name { get; }
        public string Value { get; }
    }
}
