using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Abstractions.Http.Header;
using MiniWebServer.Server.Abstractions;
using MiniWebServer.Server.Abstractions.Http;
using MiniWebServer.Server.Abstractions.Parsers;
using MiniWebServer.Server.Abstractions.Parsers.Http11;
using MiniWebServer.Server.Http;
using MiniWebServer.Server.ProtocolHandlers.Http11;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Security.Principal;
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

    public partial class Http2ProtocolHandler(ILoggerFactory loggerFactory, IHttpComponentParser httpComponentParser, ICookieValueParser cookieValueParser) : IProtocolHandler
    {
        private const uint DefaultMaxFrameSize = 16384; // frame size = 2^14 unless when you change it with a SETTINGS frame
        private const uint DefaultInitialWindowSize = 65535;
        private const uint DefaultHeaderTableSize = 4096;

        private readonly ILogger<Http2ProtocolHandler> logger = loggerFactory.CreateLogger<Http2ProtocolHandler>();

        public int ProtocolVersion => 20;

        // connection settings
        private uint maxFrameSize = DefaultMaxFrameSize;
        private uint initialWindowSize = DefaultInitialWindowSize;
        private uint headerTableSize = DefaultHeaderTableSize;
        private uint maxConcurrentStreams = 100; // can be changed by SETTINGS_MAX_CONCURRENT_STREAMS parameter in SETTINGS frame
        private uint windowSizeIncrement;

        // streams
        private readonly Http2StreamContainer inputStreamContainer = [];
        private readonly HPACKHeaderTable headerTable = new();
        private uint nextOutputStreamId = 0; // do not change this value directly, call GetNextSTreamIdentifier() when you want to get an new Id

        // for analytics
        private ulong frameCount = 0Lu; // for "telemetry"
        public Task ReadBodyAsync(PipeReader reader, IHttpRequest requestBuilder, CancellationToken cancellationToken)
        {
            // throw new NotImplementedException();

            return Task.CompletedTask;
        }

        public async Task<bool> ReadRequestAsync(PipeReader reader, IHttpRequestBuilder requestBuilder, CancellationToken cancellationToken)
        {
            try
            {
                ReadResult readResult = await reader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = readResult.Buffer;

                //requestBuilder.SetBodyPipeline(new Pipe());

                var httpMethod = HttpMethod.Get;

                var frame = new Http2Frame(); // can we reuse?

                // HTTP2 connections always start with a SETTINGS frame
                while (Http2FrameReader.TryReadFrame(ref buffer, ref frame, maxFrameSize, out ReadOnlySequence<byte> payload))
                {
#if DEBUG
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Frame found: {ft}, Stream Id: {sid}, payload length: {pll}, flags: {flags}", frame.FrameType, frame.StreamIdentifier, frame.Length, frame.Flags);
                    }
#endif

                    if (frameCount == 0 && frame.FrameType != Http2FrameType.SETTINGS)
                    {
                        logger.LogError("First frame must be a SETTINGS frame");
                        return false;
                    }

                    if (!ProcessFrame(ref frame, ref payload, logger))
                    {
                        return false;
                    }

                    frameCount = Interlocked.Increment(ref frameCount);

                    reader.AdvanceTo(buffer.Start);

                    // TODO: like HTTP1.1, we can start building requests after receiving a END_HEADERS
                    //if (frame.Flags.HasFlag(Http2FrameFlags.END_HEADERS))
                    //{
                    //    // parse header frames
                    //    if (!streamContainer.TryGetValue(frame.StreamIdentifier, out var stream))
                    //    {
                    //        logger.LogError("Stream not found: {id}", frame.StreamIdentifier);
                    //    }
                    //    else
                    //    {
                    //        var headers = BuildHeaders(stream);

                    //    }
                    //}

                    if (frame.Flags.HasFlag(Http2FrameFlags.END_STREAM))
                    {
                        // start building a request

                        if (!inputStreamContainer.Remove(frame.StreamIdentifier, out var stream))
                        {
                            logger.LogError("Stream not found: {id}", frame.StreamIdentifier);
                            return false;
                        }

                        var b = BuildRequestFromStream(stream!, requestBuilder);

                        return b;
                    }
                }

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

        private static List<HPACKHeader> BuildHeaders(Http2Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var headers = new List<HPACKHeader>();
            foreach (var headerFrame in stream.HeaderPayloads)
            {
                headers.AddRange(headerFrame.Headers);
            }

            return headers;
        }

        private bool BuildRequestFromStream(Http2Stream stream, IHttpRequestBuilder requestBuilder)
        {
            requestBuilder.SetHttpVersion(HttpVersions.Http20);

            var hpackHeaders = BuildHeaders(stream);

            if (!DecodeHeaders(hpackHeaders, requestBuilder))
            {
                return false;
            }

            return true;
        }

        private bool DecodeHeaders(IEnumerable<HPACKHeader> hpackHeaders, IHttpRequestBuilder requestBuilder)
        {
            // convert encoded headers from a frame to HTTP regular headers

            foreach (var hpackHeader in hpackHeaders)
            {
                if (hpackHeader.HeaderType == HPACKHeaderTypes.Static)
                {
                    var staticHeader = HPACKStaticTable.GetHeader(hpackHeader.StaticTableIndex) ?? throw new InvalidOperationException("Invalid HPACK static index");
                    switch (staticHeader.StaticTableIndex) // https://httpwg.org/specs/rfc7541.html#static.table.definition, refer HPACKStaticTable for more information
                    {
                        case 1:
                            if (HostHeader.TryParse(hpackHeader.Value, out var host))
                            {
                                requestBuilder.RequestHeaders.Host = host;
                                requestBuilder.SetHost(host!.Host);
                                requestBuilder.SetPort(host!.Port);
                            }
                            else
                            {
                                throw new InvalidHeaderException(new HttpHeader("Host", hpackHeader.Value));
                            }
                            break;
                        case 2:
                            requestBuilder.SetMethod(HttpMethod.Get);
                            break;
                        case 3:
                            requestBuilder.SetMethod(HttpMethod.Post);
                            break;
                        case 4:
                            if (hpackHeader.Value.Length == 0)
                            {
                                logger.LogError("Header 4 cannot be empty");
                                return false;
                            }
                            if (httpComponentParser.TryParseUrl(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes(hpackHeader.Value)), out string? url, out string? hash, out string? queryString, out string[]? segments, out HttpParameters? parameters))
                            {
                                requestBuilder.SetUrl(url!);
                                requestBuilder.SetHash(hash!);
                                requestBuilder.SetQueryString(queryString!);
                                requestBuilder.SetSegments(segments!);
                                requestBuilder.SetParameters(parameters!);
                            }
                            else
                            {
                                logger.LogError("Error parsing header 4: {p}", hpackHeader.Value);
                                return false;
                            }
                            break;
                        case 5:
                            requestBuilder.SetUrl(staticHeader.Value);
                            break;
                        case 6:
                            requestBuilder.SetHttps(false);
                            break;
                        case 7:
                            requestBuilder.SetHttps(true);
                            break;
                        case >= 8 and <= 14:
                            // from 8-14 are response statuses
                            break;
                        case 15:
                            // Accept-Charset: Do not use this header. Browsers omit this header and servers should ignore it. (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Charset)
                            break;
                        case 16:
                            requestBuilder.AddHeader(staticHeader.Name, staticHeader.Value);
                            break;
                        case >= 17:
                            requestBuilder.AddHeader(staticHeader.Name, hpackHeader.Value);
                            break;
                        default:
                            throw new NotImplementedException($"Header index not supported: {staticHeader.StaticTableIndex}"); // TODO: this should not happen, I made this to remind myself about this incompletion
                    }
                }
                else if (hpackHeader.HeaderType == HPACKHeaderTypes.Literal)
                {
                    requestBuilder.AddHeader(hpackHeader.Name, hpackHeader.Value);
                }
            }

            return true;
        }

        private bool ProcessFrame(ref Http2Frame frame, ref ReadOnlySequence<byte> payload, ILogger logger)
        {
            var b = false;

            switch (frame.FrameType)
            {
                case Http2FrameType.HEADERS:
                    b = ProcessHEADERSFrame(ref frame, ref payload, logger);
                    break;
                case Http2FrameType.SETTINGS:
                    b = ProcessSETTINGSFrame(ref frame, ref payload);
                    break;
                case Http2FrameType.WINDOW_UPDATE:
                    b = ProcessWINDOW_UPDATEFrame(ref frame, ref payload);
                    break;
                default:
                    logger.LogError("Unknown frame type: {ft}, stream: {sid}", frame.FrameType, frame.StreamIdentifier);
                    break;
            }

            return b;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> WriteResponseAsync(IHttpResponse response, CancellationToken cancellationToken)
        {
            var streamId = GetNextStreamIdentifier();
            var stream = response.Stream ?? throw new InvalidOperationException("response.Stream should not be null");

            await WriteResponseAsync(streamId, response, stream);

            return true;
        }

        private async Task WriteResponseAsync(uint streamId, IHttpResponse response, Stream stream)
        {
            var headerFrame = new Http2Frame()
            {
                StreamIdentifier = streamId,
                FrameType = Http2FrameType.HEADERS,
            };

            if (await !Http2FrameWriter.SerializeHeaderFrames(streamId, response, stream))
            {

            }
        }

        private uint GetNextStreamIdentifier()
        {
            return Interlocked.Add(ref nextOutputStreamId, 2); // an Id initiated by server is always an even 31-bit number 
        }
    }
}
