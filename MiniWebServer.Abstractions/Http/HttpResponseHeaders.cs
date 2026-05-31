namespace MiniWebServer.Abstractions.Http;

public class HttpResponseHeaders : HttpHeaders
{
    public HttpResponseHeaders()
    {
        ContentLength = 0;
    }
    public long ContentLength
    {
        get
        {
            var v = TryGetValueAsString("Content-Length");
            if (v == null)
                return 0;

            return long.Parse(v);
        }
        set
        {
            AddOrUpdate("Content-Length", value.ToString());
        }
    }

    public string? Connection
    {
        get
        {
            return TryGetValueAsString("Connection");
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            AddOrUpdate("Connection", value);
        }
    }
    public string? ContentType
    {
        get
        {
            return TryGetValueAsString("Content-Type");
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            AddOrUpdate("Content-Type", value);
        }
    }
    public string? ContentEncoding
    {
        get
        {
            return TryGetValueAsString("Content-Encoding");
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            AddOrUpdate("Content-Encoding", value);
        }
    }

    public string? Location
    {
        get
        {
            return TryGetValueAsString("Location");
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            AddOrUpdate("Location", value);
        }
    }

    public string? SecWebSocketAccept
    {
        get
        {
            return TryGetValueAsString("Sec-WebSocket-Accept");
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            AddOrUpdate("Sec-WebSocket-Accept", value);
        }
    }

    private string? TryGetValueAsString(string name, string? defaultValue = null)
    {
        if (TryGetValue(name, out var value))
        {
            if (value == null)
                return defaultValue;

            return value.Value.FirstOrDefault(defaultValue);
        }
        else
        {
            return defaultValue;
        }

    }
}
