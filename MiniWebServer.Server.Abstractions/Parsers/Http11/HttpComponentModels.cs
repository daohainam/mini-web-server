using MiniWebServer.Abstractions.Http;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server.Abstractions.Parsers.Http11;

public class HttpProtocolVersion(string major, string minor)
{
    public string Major { get; } = major ?? throw new ArgumentNullException(nameof(major));
    public string Minor { get; } = minor ?? throw new ArgumentNullException(nameof(minor));

    public override string ToString()
    {
        return $"HTTP/{Major}.{Minor}";
    }
}
public class HttpRequestLine
{
    public HttpRequestLine(HttpMethod method, string url, string hash, string queryString, HttpProtocolVersion protocolVersion, string[] segments, HttpParameters? parameters = null)
    {
        Method = method ?? throw new ArgumentNullException(nameof(method));
        Url = url ?? throw new ArgumentNullException(nameof(url));
        ProtocolVersion = protocolVersion ?? throw new ArgumentNullException(nameof(protocolVersion));
        Hash = hash;
        QueryString = queryString;
        Segments = segments;

        if (parameters != null)
        {
            Parameters = new HttpParameters(parameters);
        }
        else
        {
            Parameters = [];
        }
    }

    public HttpMethod Method { get; }
    public string Url { get; }
    public string Hash { get; }
    public string QueryString { get; }
    public string[] Segments { get; }
    public HttpParameters Parameters { get; }
    public HttpProtocolVersion ProtocolVersion { get; }

    public override string ToString()
    {
        return $"{Method.Method} {Url} {ProtocolVersion}";
    }
}

public class HttpHeaderLine(string name, string value)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public string Value { get; } = value ?? throw new ArgumentNullException(nameof(value));

    public override string ToString()
    {
        return $"{Name}: {Value}";
    }
}
