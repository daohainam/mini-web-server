using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Server.Abstractions.HttpParser.Http11;
using System.Net;
using System.Text;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Xml.Linq;
using System.Net.Http;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.Http;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    public class Http11IProtocolHandler : IProtocolHandler // should we use PipeLines to make the code simpler?
    {
        public const string HttpVersionString = "HTTP/1.1";
        public const int HttpMaxRequestLineLength = 8 * 1024; // max 8KB each line
        public const int HttpMaxHeaderLineLength = 8 * 1024; // max 8KB each line
        public SortedList<string, string> allowedMethods = new() {
            { "GET", string.Empty },
            { "POST", string.Empty }
        };

        protected readonly ProtocolHandlerConfiguration config;
        protected readonly ILoggerFactory loggerFactory;
        protected readonly IHttp11Parser http11Parser;
        protected readonly IHeaderValidator[] headerValidators;

        private readonly ILogger<Http11IProtocolHandler> logger;

        public Http11IProtocolHandler(ProtocolHandlerConfiguration config, ILoggerFactory loggerFactory, IHttp11Parser? http11Parser)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.http11Parser = http11Parser ?? throw new ArgumentNullException(nameof(http11Parser));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            logger = loggerFactory.CreateLogger<Http11IProtocolHandler>();

            headerValidators = new List<IHeaderValidator>() {
                new Http11StandardHeaderValidators.ContentLengthHeaderValidator(config.MaxRequestBodySize, loggerFactory),
                new Http11StandardHeaderValidators.TransferEncodingHeaderValidator(loggerFactory),
            }.ToArray();
        }

        public int ProtocolVersion => 101;

        public async Task<bool> ReadRequestAsync(PipeReader reader, IHttpRequestBuilder requestBuilder, CancellationToken cancellationToken)
        {
            long contentLength = 0;
            var httpMethod = HttpMethod.Get;
            string contentType = string.Empty;

            ReadResult readResult = await reader.ReadAsync(cancellationToken);
            ReadOnlySequence<byte> buffer = readResult.Buffer;

            requestBuilder.SetBodyPipeline(new Pipe());

            // read request line
            if (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                // we do some quick checks here to reject 'easy-to-check' invalid requests
                if (line.Length > HttpMaxRequestLineLength)
                {
                    logger.LogError("Request line too long");
                    return false;
                }

                if (line.Length < 14) // no request line is shorter than 'GET / HTTP/1.1' 
                {
                    logger.LogError("Request line too short");
                    return false;
                }

                string firstChars = Encoding.ASCII.GetString(line.Slice(0, 5));
                int idx = firstChars.IndexOf(' ');
                if (idx < 0)
                {
                    logger.LogError("Invalid method");
                    return false;
                }
                else
                {
                    if (!allowedMethods.ContainsKey(firstChars[..idx]))
                    {
                        logger.LogError("Not supported method");
                        return false;
                    }
                }

                var sb = new StringBuilder();
                sb.Append(Encoding.ASCII.GetString(line).Replace("\r", ""));
                var requestLineText = sb.ToString();
                var requestLine = http11Parser.ParseRequestLine(requestLineText);

                if (requestLine != null)
                {
                    logger.LogDebug("Parsed request line: {requestLine}", requestLine);

                    var method = GetHttpMethod(requestLine.Method);

                    // when implementing as a sequence of octets, if method length exceeds method buffer length, you should return 501 Not Implemented 
                    // if Url length exceeds Url buffer length, you should return 414 URI Too Long

                    // todo: parse the Url with percent-encoding (https://www.rfc-editor.org/rfc/rfc3986)

                    httpMethod = method;
                    requestBuilder
                        .SetMethod(method)
                        .SetUrl(requestLine.Url)
                        .SetParameters(requestLine.Parameters)
                        .SetQueryString(requestLine.QueryString)
                        .SetHash(requestLine.Hash);
                }
                else
                {
                    logger.LogError("Invalid request line: {line}", sb.ToString());

                    return false;
                }

                reader.AdvanceTo(buffer.Start); // after a successful TryReadLine, buffer.Start advanced to the byte after '\n'
            }

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
                    var headerLine = http11Parser.ParseHeaderLine(headerLineText);

                    logger.LogDebug("Found header: {headerLine}", headerLine);

                    if (headerLine != null)
                    {
                        if (!IsValidHeader(headerLine.Name, headerLine.Value))
                        {
                            logger.LogError("Header line rejected: {line}", headerLineText);

                            return false;
                        }

                        requestBuilder.AddHeader(headerLine.Name, headerLine.Value);

                        // here we have some checks for important headers
                        if ("Content-Length".Equals(headerLine.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (long.TryParse(headerLine.Value, out long length) && length >= 0)
                            {
                                contentLength = length;
                                requestBuilder.SetContentLength(length);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if ("Content-Type".Equals(headerLine.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            contentType = headerLine.Value;
                            requestBuilder.SetContentType(contentType);
                        }
                        else if ("Cookie".Equals(headerLine.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var cookies = http11Parser.ParseCookieHeader(headerLine.Value);
                            if (cookies == null)
                            {
                                logger.LogError("Error parsing cookie value: {cookie}", headerLine.Value);
                                return false;
                            }
                            else
                            {
                                requestBuilder.AddCookie(cookies);
                            }
                        }
                    }
                    else
                    {
                        logger.LogError("Invalid header line: {line}", headerLineText);

                        return false;
                    }

                    reader.AdvanceTo(buffer.Start);
                }
            }

            return true;
        }

        public async Task ReadBodyAsync(PipeReader reader, IHttpRequest request, CancellationToken cancellationToken)
        {
            // we don't read body part in ReadRequestAsync, because:
            // 1. a body can be very large, and we want to read/process it only when an app requests
            // 2. we want to make it responsive, we can discard a connection right away without reading it's body, there is no reasons to waste our resouces to process an invalid request

            // read body part, we read only contentLength bytes
            ReadResult readResult = await reader.ReadAsync(cancellationToken);
            ReadOnlySequence<byte> buffer = readResult.Buffer;
            var contentLength = request.ContentLength;

            long bytesRead = 0;
            while (bytesRead < contentLength)
            {
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
                if (contentLength == 0)
                {
                    // POST and PUT require body part
                    logger.LogError("POST and GET require Content-Length > 0");

                    return false;
                }
                if (string.IsNullOrEmpty(contentType))
                {
                    logger.LogError("Content-Type cannot be empty");

                    return false;
                }
            }


            return true;
        }

        protected bool IsValidHeader(string name, string value)
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

        private static void Write(IBufferWriter<byte> writer, string s)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            writer.Write(bytes.AsSpan());
        }

        public async Task<bool> WriteResponseAsync(IBufferWriter<byte> writer, IHttpResponse response, CancellationToken cancellationToken)
        {
            Write(writer, $"{HttpVersionString} {((int)response.StatusCode)} {response.ReasonPhrase}\r\n");
            foreach (var header in response.Headers)
            {
                Write(writer, $"{header.Key}: {string.Join(',', header.Value.Value)}\r\n");
            }
            foreach (var cookie in response.Cookies)
            {
                Write(writer, $"Set-Cookie: {string.Join("; ", cookie.Value.Value)}\r\n");
            }
            Write(writer, "\r\n");

            await response.Content.WriteToAsync(writer, cancellationToken); // todo: what if we have an error while sending response content?

            return true;
        }

        private static HttpMethod GetHttpMethod(string method)
        {
            if (HttpMethod.Connect.Method == method)
                return HttpMethod.Connect;
            else if (HttpMethod.Delete.Method == method)
                return HttpMethod.Delete;
            else if (HttpMethod.Get.Method == method)
                return HttpMethod.Get;
            else if (HttpMethod.Head.Method == method)
                return HttpMethod.Head;
            else if (HttpMethod.Options.Method == method)
                return HttpMethod.Options;
            else if (HttpMethod.Post.Method == method)
                return HttpMethod.Post;
            else if (HttpMethod.Put.Method == method)
                return HttpMethod.Put;
            else if (HttpMethod.Trace.Method == method)
                return HttpMethod.Trace;

            throw new ArgumentException("Unknown method", nameof(method));
        }

        public void Reset()
        {
        }
    }
}
