using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Server.Abstractions.HttpParser.Http11
{
    public interface IHttpComponentParser
    {
        HttpRequestLine? ParseRequestLine(string text);
        HttpRequestLine? ParseRequestLine(ReadOnlySequence<byte> lineBytes);
        HttpHeaderLine? ParseHeaderLine(string text);
        IEnumerable<HttpCookie>? ParseCookieHeader(string value);
    }
}
