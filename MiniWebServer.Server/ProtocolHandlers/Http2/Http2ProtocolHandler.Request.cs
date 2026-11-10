using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Abstractions.Http.Header;
using MiniWebServer.Server.Abstractions.Http;
using MiniWebServer.Server.Http;
using System.Buffers;
using System.Text;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server.ProtocolHandlers.Http2;

public partial class Http2ProtocolHandler
{
    // RFC 9113 8.2.2: connection-specific header fields are not allowed in HTTP/2
    private static readonly HashSet<string> ConnectionSpecificHeaders = new(StringComparer.Ordinal)
    {
        "connection", "keep-alive", "proxy-connection", "transfer-encoding", "upgrade"
    };

    /// <summary>
    /// Turns a stream with a complete header block into a request. Malformed requests are answered with RST_STREAM and skipped.
    /// </summary>
    private async Task<bool> TryBuildRequestAsync(Http2Stream stream, IHttpRequestBuilder requestBuilder, CancellationToken cancellationToken)
    {
        if (stream.IsReset || !streams.ContainsKey(stream.StreamId))
        {
            return false; // reset before it could be dispatched
        }

        try
        {
            BuildRequest(stream, requestBuilder);
        }
        catch (Http2StreamException ex)
        {
            await HandleStreamErrorAsync(ex, cancellationToken);
            return false;
        }

        streamsByBody[stream.Body] = stream;

        return true;
    }

    private void BuildRequest(Http2Stream stream, IHttpRequestBuilder requestBuilder)
    {
        // RFC 9113 8.3: pseudo-header fields come first, exactly once each, and only the request ones are allowed
        string? method = null, scheme = null, path = null, authority = null;
        bool pseudoHeadersDone = false;
        List<string>? cookies = null;
        var regularHeaders = new List<HttpHeader>(stream.Headers.Count);

        foreach (var header in stream.Headers)
        {
            if (header.Name.Length == 0)
            {
                throw Malformed(stream, "Empty header field name");
            }

            if (header.Name[0] == ':')
            {
                if (pseudoHeadersDone)
                {
                    throw Malformed(stream, "Pseudo-header field after a regular header field");
                }

                switch (header.Name)
                {
                    case HPACKStaticTable.METHOD:
                        method = SetOnce(stream, method, header);
                        break;
                    case HPACKStaticTable.SCHEME:
                        scheme = SetOnce(stream, scheme, header);
                        break;
                    case HPACKStaticTable.PATH:
                        path = SetOnce(stream, path, header);
                        break;
                    case HPACKStaticTable.AUTHORITY:
                        authority = SetOnce(stream, authority, header);
                        break;
                    default:
                        throw Malformed(stream, $"Unknown or disallowed pseudo-header field {header.Name}");
                }

                continue;
            }

            pseudoHeadersDone = true;
            ValidateFieldName(stream, header.Name);
            ValidateFieldValue(stream, header);

            if (ConnectionSpecificHeaders.Contains(header.Name))
            {
                throw Malformed(stream, $"Connection-specific header field {header.Name}");
            }

            if (header.Name == "te" && header.Value != "trailers")
            {
                throw Malformed(stream, "The te header field may only contain \"trailers\"");
            }

            if (header.Name == "cookie")
            {
                // RFC 9113 8.2.3: split cookie fields are joined back with "; "
                (cookies ??= []).Add(header.Value);
                continue;
            }

            regularHeaders.Add(new HttpHeader(header.Name, header.Value));
        }

        if (cookies != null)
        {
            regularHeaders.Add(new HttpHeader("cookie", string.Join("; ", cookies)));
        }

        if (method == null || scheme == null || path == null)
        {
            // CONNECT requests omit :scheme and :path, they end up here as well because CONNECT is not supported
            throw Malformed(stream, "Missing :method, :scheme or :path pseudo-header field");
        }

        if (method.Length == 0)
        {
            throw Malformed(stream, "Empty :method pseudo-header field");
        }

        if (path.Length == 0 || (path != "*" && path[0] != '/'))
        {
            throw Malformed(stream, "The :path pseudo-header field must be an absolute path or \"*\"");
        }

        HostHeader? host = null;
        if (authority != null && !HostHeader.TryParse(authority, out host))
        {
            throw Malformed(stream, "Invalid :authority pseudo-header field");
        }

        if (!httpComponentParser.TryParseUrl(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes(path)), out string? url, out string? hash, out string? queryString, out string[]? segments, out HttpParameters? parameters))
        {
            throw Malformed(stream, "Invalid :path pseudo-header field");
        }

        HttpRequestHeaders requestHeaders;
        try
        {
            requestHeaders = HttpRequestHeadersFactory.CreateFrom(regularHeaders);
        }
        catch (InvalidHeaderException ex)
        {
            throw Malformed(stream, $"Invalid header field: {ex.Message}");
        }

        if (host != null)
        {
            requestHeaders.Host = host; // RFC 9113 8.3.1: :authority takes precedence over host
        }

        if (requestHeaders.Host == null)
        {
            throw Malformed(stream, "Missing :authority pseudo-header field and host header field");
        }

        if (stream.RemoteClosed && stream.DeclaredContentLength is long declared && declared != stream.ReceivedBodyBytes)
        {
            throw Malformed(stream, "Request body does not match content-length");
        }

        requestBuilder
            .SetHttpVersion(HttpVersions.Http20)
            .SetMethod(new HttpMethod(method))
            .SetUrl(url!)
            .SetHash(hash!)
            .SetQueryString(queryString!)
            .SetSegments(segments!)
            .SetParameters(parameters!)
            .SetHost(requestHeaders.Host.Host)
            .SetHeaders(requestHeaders)
            .AddCookie(requestHeaders.Cookie)
            .SetContentType(requestHeaders.ContentType)
            .SetContentLength(stream.DeclaredContentLength ?? 0)
            .SetBodyPipeline(stream.Body);

        if (requestHeaders.Host.Port > 0)
        {
            requestBuilder.SetPort(requestHeaders.Host.Port);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Stream {sid}: {method} {path}", stream.StreamId, method, path);
        }
    }

    private static string SetOnce(Http2Stream stream, string? current, HPACKHeader header)
    {
        if (current != null)
        {
            throw Malformed(stream, $"Duplicate pseudo-header field {header.Name}");
        }

        return header.Value;
    }

    private static void ValidateFieldName(Http2Stream stream, string name)
    {
        // RFC 9113 8.2.1: field names are tokens and must be lowercase
        foreach (char c in name)
        {
            bool valid = c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '.' or '^' or '_' or '`' or '|' or '~';
            if (!valid)
            {
                throw Malformed(stream, $"Invalid header field name {name}");
            }
        }
    }

    private static void ValidateFieldValue(Http2Stream stream, HPACKHeader header)
    {
        // RFC 9113 8.2.1: no NUL, CR or LF, and no leading or trailing whitespace
        string value = header.Value;
        foreach (char c in value)
        {
            if (c is '\0' or '\r' or '\n')
            {
                throw Malformed(stream, $"Invalid character in the value of header field {header.Name}");
            }
        }

        if (value.Length > 0 && (value[0] is ' ' or '\t' || value[^1] is ' ' or '\t'))
        {
            throw Malformed(stream, $"Leading or trailing whitespace in the value of header field {header.Name}");
        }
    }

    private static Http2StreamException Malformed(Http2Stream stream, string message)
    {
        // RFC 9113 8.1.1: malformed requests are a stream error of type PROTOCOL_ERROR
        return new Http2StreamException(stream.StreamId, Http2ErrorCode.PROTOCOL_ERROR, message);
    }
}
