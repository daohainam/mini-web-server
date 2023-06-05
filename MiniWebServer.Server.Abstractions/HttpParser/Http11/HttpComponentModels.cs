using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Abstractions.HttpParser.Http11
{
    public class HttpProtocolVersion
    {
        public HttpProtocolVersion(string major, string minor)
        {
            Major = major ?? throw new ArgumentNullException(nameof(major));
            Minor = minor ?? throw new ArgumentNullException(nameof(minor));
        }

        public string Major { get; }
        public string Minor { get; }

        public override string ToString()
        {
            return $"HTTP/{Major}.{Minor}";
        }
    }
    public class HttpRequestLine
    {
        public HttpRequestLine(string method, string url, string hash, string queryString, HttpProtocolVersion protocolVersion, HttpParameters? parameters = null)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            ProtocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
            Hash = hash;
            QueryString = queryString;

            if (parameters != null)
            {
                Parameters = new HttpParameters(parameters);
            }
            else
            {
                Parameters = new HttpParameters();
            }
        }

        public string Method { get; }
        public string Url { get; }
        public string Hash { get; }
        public string QueryString { get; }
        public HttpParameters Parameters { get; }
        public HttpProtocolVersion ProtocolVersion { get; }

        public override string ToString()
        {
            return $"{Method} {Url} {ProtocolVersion}";
        }
    }

    public class HttpHeaderLine
    {
        public HttpHeaderLine(string name, string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name { get; }
        public string Value { get; }

        public override string ToString()
        {
            return $"{Name}: {Value}";
        }
    }
}
