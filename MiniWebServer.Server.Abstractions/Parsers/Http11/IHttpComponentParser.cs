using MiniWebServer.Abstractions.Http;
using System.Buffers;

namespace MiniWebServer.Server.Abstractions.Parsers.Http11;

public interface IHttpComponentParser
{
    /// <summary>
    /// Parse a request line (https://www.rfc-editor.org/rfc/rfc9112#name-request-line). This will also remove last CR (if exists)
    /// </summary>
    /// <param name="lineBytes"></param>
    /// <returns></returns>
    HttpRequestLine? ParseRequestLine(ReadOnlySequence<byte> lineBytes);

    /// <summary>
    /// Parse a header line (https://www.rfc-editor.org/rfc/rfc9112#name-field-syntax). This will also remove last CR (if exists)
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    HttpHeader? ParseHeaderLine(ReadOnlySequence<byte> buffer);
    bool TryParseUrl(ReadOnlySequence<byte> readOnlySequence, out string? url, out string? hash, out string? queryString, out string[]? segments, out HttpParameters? parameters);
}
