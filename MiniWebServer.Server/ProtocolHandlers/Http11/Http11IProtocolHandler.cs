using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.Http;
using MiniWebServer.Server.Abstractions.Parsers;
using MiniWebServer.Server.Abstractions.Parsers.Http11;
using MiniWebServer.Server.Host;
using MiniWebServer.Server.Http;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    public class Http11IProtocolHandler(ProtocolHandlerConfiguration config, ILoggerFactory loggerFactory, IHttpComponentParser httpComponentParser, ICookieValueParser cookieValueParser) : IProtocolHandler // should we use PipeLines to make the code simpler?
    {
        public const string HttpVersionString = "HTTP/1.1";
        public const int HttpMaxHeaderLineLength = 8 * 1024; // max 8KB each line

        protected readonly ProtocolHandlerConfiguration config = config ?? throw new ArgumentNullException(nameof(config));
        protected readonly ILoggerFactory loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        protected readonly IHttpComponentParser httpComponentParser = httpComponentParser ?? throw new ArgumentNullException(nameof(httpComponentParser));
        private readonly ICookieValueParser cookieValueParser = cookieValueParser ?? throw new ArgumentNullException(nameof(cookieValueParser));
        protected readonly IHeaderValidator[] headerValidators = [
                new Http11StandardHeaderValidators.ContentLengthHeaderValidator(config.MaxRequestBodySize, loggerFactory),
                new Http11StandardHeaderValidators.TransferEncodingHeaderValidator(loggerFactory),
            ];

        private readonly ILogger<Http11IProtocolHandler> logger = loggerFactory.CreateLogger<Http11IProtocolHandler>();

        public int ProtocolVersion => 101;

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
                // read request line
                if (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
                {
                    var requestLine = httpComponentParser.ParseRequestLine(line);

                    if (requestLine != null)
                    {
                        logger.LogDebug("Parsed request line: {requestLine}", requestLine);

                        // when implementing as a sequence of octets, if method length exceeds method buffer length, you should return 501 Not Implemented 
                        // if Url length exceeds Url buffer length, you should return 414 URI Too Long

                        // todo: parse the Url with percent-encoding (https://www.rfc-editor.org/rfc/rfc3986)

                        httpMethod = requestLine.Method;
                        requestBuilder
                            .SetMethod(requestLine.Method)
                            .SetUrl(requestLine.Url)
                            .SetParameters(requestLine.Parameters)
                            .SetQueryString(requestLine.QueryString)
                            .SetHash(requestLine.Hash)
                            .SetSegments(requestLine.Segments);
                    }
                    else
                    {
                        logger.LogError("Invalid request line");

                        return false;
                    }

                    reader.AdvanceTo(buffer.Start); // after a successful TryReadLine, buffer.Start advanced to the byte after '\n'
                }
                else
                {
                    // we could not read header line
                    return false;
                }

                var headers = new List<HttpHeader>();
                // now we read headers
                while (TryReadLine(ref buffer, out line))
                {
                    if (line.Length > HttpMaxHeaderLineLength)
                    {
                        logger.LogError("Header line too long");
                        return false;
                    }

                    var sb = new StringBuilder();
                    sb.Append(Encoding.ASCII.GetString(line).Replace("\r", ""));

                    if (sb.Length == 0) // found an empty line
                    {
                        if (!ValidateRequestHeader(httpMethod, contentLength, contentType))
                        {
                            return false; // reject request for whatever reason
                        }

                        reader.AdvanceTo(buffer.Start);
                        break;
                    }
                    else
                    {
                        var headerLineText = sb.ToString();
                        var header = httpComponentParser.ParseHeaderLine(line);

                        logger.LogDebug("Found header: {headerLine}", header);

                        if (header != null)
                        {
                            if (!IsValidHeader(header.Name, header.Value))
                            {
                                logger.LogError("Header line rejected: {line}", headerLineText);

                                return false;
                            }

                            headers.Add(header);
                        }
                        else
                        {
                            logger.LogError("Invalid header line: {line}", headerLineText);

                            return false;
                        }

                        reader.AdvanceTo(buffer.Start);
                    }
                }

                // here we have some checks for important headers
                var requestHeaders = HttpRequestHeadersFactory.CreateFrom(headers);
                if (requestHeaders.Host != null)
                {
                    requestBuilder.SetHost(requestHeaders.Host.Host);
                    if (requestHeaders.Host.Port != 0)
                        requestBuilder.SetPort(requestHeaders.Host.Port);
                }

                requestBuilder.AddCookie(requestHeaders.Cookie);
                requestBuilder.AddTransferEncoding(requestHeaders.TransferEncoding);
                requestBuilder.SetContentLength(contentLength);
                requestBuilder.SetContentType(contentType);

                requestBuilder.SetHeaders(requestHeaders);

                return true;
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

        public async Task ReadBodyAsync(PipeReader reader, IHttpRequest request, CancellationToken cancellationToken)
        {
            // we don't read body part in ReadRequestAsync, because:
            // 1. a body can be very large, and we want to read/process it only when an app requests
            // 2. we want to make it responsive, we can discard a connection right away without reading it's body, there is no reasons to waste our resouces to process an invalid request

            // read body part, we read only contentLength bytes
            var contentLength = request.ContentLength;
            if (contentLength == 0)
            {
                // nothing to read
                await request.BodyPipeline.Writer.CompleteAsync();
                return;
            }
            if (contentLength < 0)
            {
                throw new InvalidOperationException("Content-Length < 0");
            }
            logger.LogDebug("Start reading body, contentLength = {contentLength}", contentLength);

            ReadResult readResult = await reader.ReadAsync(cancellationToken);
            ReadOnlySequence<byte> buffer = readResult.Buffer;

            long bytesRead = 0;

            while (bytesRead < contentLength)
            {
                logger.LogDebug("Reading body, bytesRead = {bytesRead}", bytesRead);

                long maxBytesToRead = contentLength - bytesRead;
                if (buffer.Length >= maxBytesToRead)
                {
                    var writingPart = buffer.Slice(0, maxBytesToRead);
                    await request.BodyPipeline.Writer.WriteAsync(writingPart.ToArray(), cancellationToken); // todo: use a better way than 'ToArray'

                    reader.AdvanceTo(buffer.GetPosition(maxBytesToRead));

                    //logger.LogDebug("Read {b} bytes from body, total {t}", buffer.Length, bytesRead);

                    break;
                }
                else if (buffer.Length > 0)
                {

                    await request.BodyPipeline.Writer.WriteAsync(buffer.ToArray(), cancellationToken); // todo: use a better way than 'ToArray'

                    reader.AdvanceTo(buffer.GetPosition(buffer.Length));

                    bytesRead += buffer.Length;
                    //logger.LogDebug("Read {b} bytes from body, total {t}", buffer.Length, bytesRead);
                }

                readResult = await reader.ReadAsync(cancellationToken);
                buffer = readResult.Buffer;
            }

            logger.LogDebug("Done reading body, bytesRead = {bytesRead}", bytesRead);

            await request.BodyPipeline.Writer.FlushAsync(cancellationToken);
            await request.BodyPipeline.Writer.CompleteAsync();
        }

        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            SequencePosition? pos = buffer.PositionOf((byte)'\n');

            if (pos != null)
            {
                line = buffer.Slice(0, pos.Value);
                buffer = buffer.Slice(buffer.GetPosition(1, pos.Value));
                return true;
            }
            else
            {
                line = default;
                return false;
            }
        }

        private bool ValidateRequestHeader(HttpMethod httpMethod, long contentLength, string contentType)
        {
            if ((httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put))
            {
                if (string.IsNullOrEmpty(contentType))
                {
                    logger.LogError("Content-Type cannot be empty");

                    return false;
                }
            }


            return true;
        }

        protected bool IsValidHeader(string name, IEnumerable<string> value)
        {
            foreach (var validator in headerValidators)
            {
                if (validator.Validate(name, value) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private static void Write(Stream stream, string s)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            stream.Write(bytes.AsSpan());
        }

        //public async Task<bool> WriteResponseAsync(PipeWriter writer, IHttpResponse response, CancellationToken cancellationToken)
        //{
        //    Write(writer, $"{HttpVersionString} {((int)response.StatusCode)} {response.ReasonPhrase}\r\n");

        //    foreach (var header in response.Content.Headers)
        //        response.Headers.AddOrUpdate(header.Key, header.Value);

        //    foreach (var header in response.Headers)
        //    {
        //        Write(writer, $"{header.Key}: {string.Join(',', header.Value)}\r\n");
        //    }
        //    foreach (var cookie in response.Cookies)
        //    {
        //        Write(writer, $"Set-Cookie: {cookie.Key}={string.Join("; ", cookie.Value.Value)}\r\n");
        //    }
        //    Write(writer, "\r\n");

        //    var contentWriter = new ByteBufferContentWriter(writer);
        //    await response.Content.WriteToAsync(contentWriter, cancellationToken); // todo: what if we have an error while sending response content?

        //    return true;
        //}

        public async Task<bool> WriteResponseAsync(IHttpResponse response, CancellationToken cancellationToken)
        {
            var stream = response.Stream ?? throw new InvalidOperationException("response.Body should not be null");
            Write(stream, $"{HttpVersionString} {((int)response.StatusCode)} {response.ReasonPhrase}\r\n");

            foreach (var header in response.Content.Headers)
                response.Headers.AddOrUpdate(header.Key, header.Value);

            foreach (var header in response.Headers)
            {
                Write(stream, $"{header.Key}: {string.Join(',', header.Value)}\r\n");
            }
            foreach (var cookie in response.Cookies)
            {
                Write(stream, $"Set-Cookie: {cookie.Key}={string.Join("; ", cookie.Value.Value)}\r\n");
            }
            Write(stream, "\r\n");

            await response.Content.WriteToAsync(stream, cancellationToken); // todo: what if we have an error while sending response content?
            await stream.FlushAsync(cancellationToken);

            return true;
        }


        public void Reset()
        {
        }


    }
}
