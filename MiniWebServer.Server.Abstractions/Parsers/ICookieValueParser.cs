using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Server.Abstractions.Parsers;

public interface ICookieValueParser
{
    IEnumerable<HttpCookie>? ParseCookieHeader(string value);

}
