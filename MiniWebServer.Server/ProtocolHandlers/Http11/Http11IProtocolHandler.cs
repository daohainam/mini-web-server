using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Abstractions.HttpParser.Http11;
using MiniWebServer.Server.ProtocolHandlers.Storage;
using System.Net;
using System.Text;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Xml.Linq;
using System.Net.Http;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    public class Http11IProtocolHandler : IProtocolHandler // should we use PipeLines to make the code simpler?
    {
        protected readonly ProtocolHandlerConfiguration config;
        protected readonly ILogger logger;
        protected readonly IHttp11Parser http11Parser;
        protected readonly IHeaderValidator[] headerValidators;

        public Http11IProtocolHandler(ProtocolHandlerConfiguration config, ILogger? logger, IHttp11Parser? http11Parser)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.http11Parser = http11Parser ?? throw new ArgumentNullException(nameof(http11Parser));

            headerValidators = new List<IHeaderValidator>() {
                new Http11StandardHeaderValidators.ContentLengthHeaderValidator(config.MaxRequestBodySize),
                new Http11StandardHeaderValidators.TransferEncodingHeaderValidator(),
            }.ToArray();
        }

        public int ProtocolVersion => 101;

        public async Task<bool> ReadRequestAsync(PipeReader reader, IHttpRequestBuilder httpWebRequestBuilder, CancellationToken cancellationToken)
        {
            long contentLength = 0;
            var httpMethod = HttpMethod.Get;
            string contentType = string.Empty;

            ReadResult readResult = await reader.ReadAsync(cancellationToken);
            ReadOnlySequence<byte> buffer = readResult.Buffer;

            // read request line
            if (TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
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

                    httpMethod = method;
                    httpWebRequestBuilder
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

                        httpWebRequestBuilder.AddHeader(headerLine.Name, headerLine.Value);

                        if ("Content-Length".Equals(headerLine.Name)) // todo: we should use in-casesensitive compare
                        {
                            if (long.TryParse(headerLine.Value, out long length))
                            {
                                contentLength = length;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if ("Content-Type".Equals(headerLine.Name)) // todo: we should use in-casesensitive compare
                        {
                            contentType = headerLine.Value;
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

            // read body part, we read only contentLength bytes
            long bytesRead = 0;
            Pipe bodyPipeline = new();
            while (bytesRead < contentLength)
            {
                long maxBytesToRead = contentLength - bytesRead;
                if (buffer.Length >= maxBytesToRead)
                {
                    var writingPart = buffer.Slice(0, maxBytesToRead);
                    await bodyPipeline.Writer.WriteAsync(writingPart.ToArray(), cancellationToken); // todo: use a better way than 'ToArray'

                    reader.AdvanceTo(new SequencePosition(buffer, (int)maxBytesToRead));

                    break;
                }
                else if (buffer.Length > 0)
                {
                    await bodyPipeline.Writer.WriteAsync(buffer.ToArray(), cancellationToken); // todo: use a better way than 'ToArray'

                    reader.AdvanceTo(new SequencePosition(buffer, (int)buffer.Length));

                    bytesRead += buffer.Length;
                }

                readResult = await reader.ReadAsync(cancellationToken);
                buffer = readResult.Buffer;
            }

            return true;
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

        private static bool ValidateRequestHeader(HttpMethod httpMethod, long contentLength, string contentType)
        {
            if ((httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put))
            {
                if (contentLength == 0)
                {
                    // POST and PUT require body part
                    return false;
                }
                if (string.IsNullOrEmpty(contentType))
                {
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

        public async Task<bool> WriteResponseAsync(IBufferWriter<byte> writer, HttpResponse response, CancellationToken cancellationToken)
        {
            Write(writer, $"HTTP/1.1 {((int)response.StatusCode)} {response.ReasonPhrase}\r\n");
            foreach (var header in response.Headers)
            {
                Write(writer, $"{header.Key}: {string.Join(',', header.Value.Value)}\r\n");
            }
            Write(writer, "\r\n");

            await response.Content.WriteToAsync(writer, cancellationToken);

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

    public enum Http11RequestMessageParts
    {
        RequestLine,
        Header,
        Body,
        Done
    }

    public enum Http11ResponseMessageParts
    {
        StatusLine,
        Header, // we will normally write status and header at the same time so we don't use this
        Body,
        Done
    }
}
