using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.Http;
using MiniWebServer.Server.Abstractions.Parsers;
using MiniWebServer.Server.Abstractions.Parsers.Http11;
using MiniWebServer.Server.Http;
using MiniWebServer.Server.ProtocolHandlers.Http11;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    /* 
     * protocol handler for http2 based on RFC 9113 (https://datatracker.ietf.org/doc/html/rfc9113)
     * 
     * Some problems we must solve before going online:
     * - find a way to switch between HTTP11 and HTTP2 (will it be handled in protocol handler factory in or internally inside HTTP2 protocol handler?)
     * - how do we handle frame buffers effectively? will it need to be shared between threads?
     * - a buffer will be controlled inside or outside protocol handlers?
     * 
     * Note:
     * - use buffer pools 
     * 
     */

    public class Http2ProtocolHandler(ILoggerFactory loggerFactory, IHttpComponentParser httpComponentParser, ICookieValueParser cookieValueParser) : IProtocolHandler
    {
        private const int DefaultMaxFrameSize = 16384; // frame size = 2^14 unless when you change it with a SETTINGS frame

        private readonly ILoggerFactory loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        private readonly ILogger<Http2ProtocolHandler> logger = loggerFactory.CreateLogger<Http2ProtocolHandler>();

        public int ProtocolVersion => 20;
        private Dictionary<uint, Http2Stream> streams = new(); // we don't use concurrent dictionary because we will implement our own sync merchanism
        private int maxFrameSize = DefaultMaxFrameSize;

        public Task ReadBodyAsync(PipeReader reader, IHttpRequest requestBuilder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ReadRequestAsync(PipeReader reader, IHttpRequestBuilder requestBuilder, CancellationToken cancellationToken)
        {
            long contentLength = 0;
            string contentType = string.Empty;

            try
            {
                ReadResult readResult = await reader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = readResult.Buffer;

                requestBuilder.SetBodyPipeline(new Pipe());

                var httpMethod = HttpMethod.Get;

                var frame = new Http2Frame(); // can we reuse?

                // read request line
                if (Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload))
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Frame found: {f}, Stream Id: {sid}", frame.FrameType, frame.StreamIdentifier);
                    }

                    switch (frame.FrameType)
                    {
                        case Http2FrameType.HEADERS:

                            break;
                    }


                    //var requestLine = httpComponentParser.ParseRequestLine(line);

                    //if (requestLine != null)
                    //{
                    //    logger.LogDebug("Parsed request line: {requestLine}", requestLine);

                    //    // when implementing as a sequence of octets, if method length exceeds method buffer length, you should return 501 Not Implemented 
                    //    // if Url length exceeds Url buffer length, you should return 414 URI Too Long

                    //    // todo: parse the Url with percent-encoding (https://www.rfc-editor.org/rfc/rfc3986)

                    //    httpMethod = requestLine.Method;
                    //    requestBuilder
                    //        .SetMethod(requestLine.Method)
                    //        .SetUrl(requestLine.Url)
                    //        .SetParameters(requestLine.Parameters)
                    //        .SetQueryString(requestLine.QueryString)
                    //    .SetHash(requestLine.Hash)
                    //        .SetSegments(requestLine.Segments);
                    //}
                    //else
                    //{
                    //    logger.LogError("Invalid request line");

                    //    return false;
                    //}

                    //reader.AdvanceTo(buffer.Start); // after a successful TryReadLine, buffer.Start advanced to the byte after '\n'
                }
                else
                {
                    // we could not read header line
                    return false;
                }

                //var headers = new List<HttpHeader>();
                //// now we read headers
                //while (TryReadLine(ref buffer, out line))
                //{
                //    if (line.Length > HttpMaxHeaderLineLength)
                //    {
                //        logger.LogError("Header line too long");
                //        return false;
                //    }

                //    var sb = new StringBuilder();
                //    sb.Append(Encoding.ASCII.GetString(line).Replace("\r", ""));

                //    if (sb.Length == 0) // found an empty line
                //    {
                //        if (!ValidateRequestHeader(httpMethod, contentLength, contentType))
                //        {
                //            return false; // reject request for whatever reason
                //        }

                //        reader.AdvanceTo(buffer.Start);
                //        break;
                //    }
                //    else
                //    {
                //        var headerLineText = sb.ToString();
                //        var header = httpComponentParser.ParseHeaderLine(line);

                //        logger.LogDebug("Found header: {headerLine}", header);

                //        if (header != null)
                //        {
                //            if (!IsValidHeader(header.Name, header.Value))
                //            {
                //                logger.LogError("Header line rejected: {line}", headerLineText);
                //                return false;
                //            }

                //            headers.Add(header);
                //        }
                //        else
                //        {
                //            logger.LogError("Invalid header line: {line}", headerLineText);

                //            return false;
                //        }

                //        reader.AdvanceTo(buffer.Start);
                //    }
                //}

                //// here we have some checks for important headers
                //var requestHeaders = HttpRequestHeadersFactory.CreateFrom(headers);
                //if (requestHeaders.Host != null)
                //{
                //    requestBuilder.SetHost(requestHeaders.Host.Host);
                //    if (requestHeaders.Host.Port != 0)
                //        requestBuilder.SetPort(requestHeaders.Host.Port);
                //}

                //requestBuilder.AddCookie(requestHeaders.Cookie);
                //requestBuilder.AddTransferEncoding(requestHeaders.TransferEncoding);
                //requestBuilder.SetContentLength(contentLength);
                //requestBuilder.SetContentType(contentType);

                //requestBuilder.SetHeaders(requestHeaders);

                //return true;

                return false;
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Connection expired");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reading request");
                return false;
            }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteResponseAsync(IHttpResponse response, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
