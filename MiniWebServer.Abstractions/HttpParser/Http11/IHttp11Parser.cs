using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.HttpParser.Http11
{
    public interface IHttp11Parser
    {
        Http11RequestLine? ParseRequestLine(string text);
        Http11HeaderLine? ParseHeaderLine(string text);
    }
}
