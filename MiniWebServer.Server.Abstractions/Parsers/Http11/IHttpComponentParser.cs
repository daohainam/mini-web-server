using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Server.Abstractions.Parsers.Http11
{
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
        HttpHeaderLine? ParseHeaderLine(ReadOnlySequence<byte> buffer);
    }
}
