using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Abstractions.HttpParser.Http11;
using System.Net;
using System.Text;
using static MiniWebServer.Abstractions.ProtocolHandlerStates;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    public class Http11IProtocolHandler : IProtocolHandler
    {
        protected readonly ILogger logger;
        protected readonly IHttp11Parser http11Parser;
        protected readonly IHeaderValidator[] headerValidators = { 
            new Http11StandardHeaderValidators.ContentLengthHeaderValidator(),
            new Http11StandardHeaderValidators.TransferEncodingHeaderValidator(),
        };

        public Http11IProtocolHandler(ILogger? logger, IHttp11Parser? http11Parser)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.http11Parser = http11Parser ?? throw new ArgumentNullException(nameof(http11Parser));
        }

        public int ProtocolVersion => 101;

        public BuildRequestStates ReadRequest(Span<byte> buffer, IHttpRequestBuilder httpWebRequestBuilder, ProtocolHandlerData data, out int bytesProcessed)
        {
            BuildRequestStates state = BuildRequestStates.InProgress;

            data.Data ??= new Http11ProtocolData(
                );

            var d = (Http11ProtocolData)data.Data;

            if (d.CurrentReadingPart == Http11RequestMessageParts.RequestLine)
            {
                // request-line = method SP request-target SP HTTP-version
                int i = 0;
                while (i < buffer.Length)
                {
                    byte b = buffer[i];

                    if (b == '\n' || b == '\r')
                    {
                        // we will process only LF, CR (\r) can be skipped
                        if (b == '\n')
                        {
                            var requestLine = http11Parser.ParseRequestLine(d.HeaderStringBuilder.ToString());

                            if (requestLine != null)
                            {
                                logger.LogDebug("Parsed request line: {requestLine}", requestLine);

                                var method = GetHttpMethod(requestLine.Method);

                                // when implementing as a sequence of octets, if method length exceeds method buffer length, you should return 501 Not Implemented 
                                // if Url length exceeds Url buffer length, you should return 414 URI Too Long

                                d.HttpMethod = method;
                                httpWebRequestBuilder
                                    .SetMethod(method)
                                    .SetUrl(requestLine.Url)
                                    .SetParameters(requestLine.Parameters)
                                    .SetQueryString(requestLine.QueryString)
                                    .SetHash(requestLine.Hash);

                                state = BuildRequestStates.InProgress;
                                d.CurrentReadingPart = Http11RequestMessageParts.Header;
                                d.HeaderStringBuilder.Clear();

                                // why don't we continue reading headers here?
                                break;
                            }
                            else
                            {
                                logger.LogError("Invalid request line: {line}", d.HeaderStringBuilder);
                                state = BuildRequestStates.Failed;
                                break;
                            }
                        }
                    }
                    else if (b >= 32 && b <= 127) // it must be a 7-bit USASCII character, but here we denied control characters also
                    {
                        d.HeaderStringBuilder.Append((char)b);
                    }
                    else
                    {
                        state = BuildRequestStates.Failed;
                        break;
                    }

                    i++;
                }

                bytesProcessed = i + 1;
            }
            else if (d.CurrentReadingPart == Http11RequestMessageParts.Header)
            {
                int i = 0;
                while (i < buffer.Length)
                {
                    byte b = buffer[i];

                    if (b == '\n' || b == '\r')
                    {
                        // we will process only LF, CR (\r) can be skipped
                        if (b == '\n')
                        {
                            if (d.HeaderStringBuilder.Length == 0) // header part ends with an empty line
                            {
                                if (!ValidateRequestHeader(d))
                                {
                                    state = BuildRequestStates.Failed; // reject request for whatever reason
                                }
                                else
                                {
                                    d.CurrentReadingPart = Http11RequestMessageParts.Body;
                                    d.RequestBodySize = 0;
                                    d.CurrentRequestBodyBytes = 0;
                                }

                                break;
                            }
                            else
                            {
                                var headerLine = http11Parser.ParseHeaderLine(d.HeaderStringBuilder.ToString());

                                logger.LogDebug("Found header: {headerLine}", headerLine);

                                if (headerLine != null)
                                {
                                    if (!IsValidHeader(headerLine.Name, headerLine.Value, d))
                                    {
                                        logger.LogError("Validating header failed: {line}", d.HeaderStringBuilder);
                                        state = BuildRequestStates.Failed;
                                        break;
                                    }

                                    httpWebRequestBuilder.AddHeader(headerLine.Name, headerLine.Value);
                                    d.RequestHeaders.Add(headerLine.Name, headerLine.Value);


                                    d.HeaderStringBuilder.Clear();
                                }
                                else
                                {
                                    logger.LogError("Invalid header line: {line}", d.HeaderStringBuilder);
                                    state = BuildRequestStates.Failed;
                                    break;
                                }
                            }
                        }
                    }
                    else if (b >= 32 && b <= 127) // it must be a 7-bit USASCII character, but here we denied control characters also
                    {
                        d.HeaderStringBuilder.Append((char)b);
                    }
                    else
                    {
                        state = BuildRequestStates.Failed;
                        break;
                    }
                    i++;
                }

                bytesProcessed = i + 1;
            }
            else
            {
                bytesProcessed = buffer.Length;
                state = BuildRequestStates.Succeeded;
            }

            return state;
        }

        private static bool ValidateRequestHeader(Http11ProtocolData d)
        {
            if ((d.HttpMethod == HttpMethod.Post || d.HttpMethod == HttpMethod.Put))
            {
                if (d.ContentLength == 0)
                { 
                    // POST and PUT require body part
                    return false;
                }
                if (string.IsNullOrEmpty(d.RequestHeaders.ContentType))
                { 
                    return false;
                }
            }


            return true;
        }

        protected bool IsValidHeader(string name, string value, Http11ProtocolData stateObject)
        {
            foreach (var validator in headerValidators)
            {
                if (validator.Validate(name, value, stateObject) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private static void Write(Stream stream, string s)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        public WriteResponseStates WriteResponse(Span<byte> buffer, HttpResponse response, ProtocolHandlerData protocolHandlerData, out int bytesProcessed)
        {
            if (protocolHandlerData.Data == null)
                throw new InvalidOperationException("WriteResponse should be called after ReadRequest");

            WriteResponseStates state = WriteResponseStates.InProgress;
            var d = (Http11ProtocolData)protocolHandlerData.Data;
            bytesProcessed = 0;

            if (d.ResponseHeaderBuffer.Length == 0)
            {
                var stream = new MemoryStream();
                // write status line and headers
                Write(stream, $"HTTP/1.1 {((int)response.StatusCode)} {response.ReasonPhrase}\r\n");
                foreach (var header in response.Headers)
                {
                    Write(stream, $"{header.Key}: {string.Join(',', header.Value.Value)}\r\n");
                }

                Write(stream, "\r\n");

                d.ResponseHeaderBuffer = stream.ToArray().AsMemory(); // don't use GetBuffer 
                d.ResponseHeaderBufferIndex = 0;
            }
            
            if (d.CurrentWritingPart == Http11ResponseMessageParts.StatusLine)
            {
                int length = Math.Min(buffer.Length, d.ResponseHeaderBuffer.Length) - d.ResponseHeaderBufferIndex;

                d.ResponseHeaderBuffer.Slice(d.ResponseHeaderBufferIndex, length).Span.CopyTo(buffer);
                bytesProcessed = length;

                d.ResponseHeaderBufferIndex = length;
                if (d.ResponseHeaderBufferIndex >= d.ResponseHeaderBuffer.Length)
                {
                    d.CurrentWritingPart = Http11ResponseMessageParts.Body;
                    d.ResponseBodyContentIndex = 0; // start writing body from beginning
                }
            }
            else if (d.CurrentWritingPart == Http11ResponseMessageParts.Body)
            {
                int bytesCopied = response.Content.CopyTo(buffer, d.ResponseBodyContentIndex);
                d.ResponseBodyContentIndex += bytesCopied;

                bytesProcessed = bytesCopied;

                if (bytesCopied == 0)
                {
                    d.CurrentWritingPart = Http11ResponseMessageParts.Done;
                    state = WriteResponseStates.Succeeded;
                }
            }

            return state;
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

        public void Reset(ProtocolHandlerData data)
        {
            if (data.Data != null)
            {
                var d = (Http11ProtocolData)data.Data;
                d.CurrentReadingPart = Http11RequestMessageParts.Header;
            }
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
