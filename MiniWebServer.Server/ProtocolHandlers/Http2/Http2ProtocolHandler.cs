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
        private const int StreamContainerLockTimeout = 3000; // 3 seconds

        private readonly ILogger<Http2ProtocolHandler> logger = loggerFactory.CreateLogger<Http2ProtocolHandler>();

        public int ProtocolVersion => 20;

        // connection settings
        private uint maxFrameSize = DefaultMaxFrameSize;
        private uint initialWindowSize = DefaultInitialWindowSize;
        private uint headerTableSize = DefaultHeaderTableSize;
        private uint maxConcurrentStreams = 100; // can be changed by SETTINGS_MAX_CONCURRENT_STREAMS parameter in SETTINGS frame
        private uint windowSizeIncrement;

        // streams
        private readonly Http2StreamContainer streamContainer = [];

        // for analytics
        private ulong frameCount = 0Lu; // for "telemetry"
        public Task ReadBodyAsync(PipeReader reader, IHttpRequest requestBuilder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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

                    if (!ProcessFrame(ref frame, ref payload, logger)) { 
                        return false; 
                    }

                    frameCount = Interlocked.Increment(ref frameCount);

                    // TODO: like HTTP1.1, we can start building requests after receiving a END_HEADERS
                    if (frame.Flags.HasFlag(Http2FrameFlags.END_HEADERS))
                    {
                        // parse header frames
                        var headers = BuildHeaders(frame.StreamIdentifier);
                    }
                    
                    if (frame.Flags.HasFlag(Http2FrameFlags.END_STREAM))
                    {
                        // start building a request

                        var request = BuildRequestFromStream(frame.StreamIdentifier);

                        if (request != null)
                        {

                        }
                        else
                        {
                            return false;
                        }
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

        private List<HPACKHeader>? BuildHeaders(uint streamIdentifier)
        {
            if (!streamContainer.TryGetValue(streamIdentifier, out var stream))
            {
                logger.LogError("Stream not found: {id}", streamIdentifier);
                return null;
            }

            foreach (var headerFrame in stream.FrameQueue.Frames)
            {
                if (headerFrame.FrameType == Http2FrameType.HEADERS)
                {

                }
            }
        }

        private HttpRequest? BuildRequestFromStream(uint streamIdentifier)
        {
            if (!streamContainer.TryRemove(streamIdentifier, out var stream))
            {
                logger.LogError("Stream not found: {id}", streamIdentifier);
                return null;
            }

            var requestBuilder = new HttpWebRequestBuilder();

            if (!DecodeHeaders(stream, requestBuilder))
            {
                return null;
            }

            return requestBuilder.Build();
        }

        private static bool DecodeHeaders(Http2Stream stream, HttpWebRequestBuilder requestBuilder)
        {
            // convert encoded headers from a frame to HTTP regular headers
            
            foreach (var headerPayload in stream.HeaderPayloads)
            {
                foreach (var header in headerPayload.Headers)
                {
                    switch (header.HeaderType)
                    {
                        case HPACKHeaderTypes.Static:
                            var staticHeader = HPACKStaticTable.GetHeader(header.StaticTableIndex) ?? throw new InvalidOperationException("Invalid HPACK static index");
                            switch (staticHeader.StaticTableIndex) // https://httpwg.org/specs/rfc7541.html#static.table.definition, refer HPACKStaticTable for more information
                            {
                                case 1:
                                    requestBuilder.SetHost(staticHeader.Value);
                                    break;
                                case 2:
                                    requestBuilder.SetMethod(HttpMethod.Get);
                                    break;
                                case 3:
                                    requestBuilder.SetMethod(HttpMethod.Post);
                                    break;
                                case 4:
                                    requestBuilder.SetUrl("/");
                                    break;
                                case 5:
                                    requestBuilder.SetUrl("/index.html");
                                    break;
                                case 6:
                                    requestBuilder.SetHttps(false);
                                    break;
                                case 7:
                                    requestBuilder.SetHttps(true);
                                    break;
                                default:
                                    throw new NotImplementedException("Header index not supported"); // TODO: this should not happen, I made this to remind myself about this incompletion
                            }
                            break;
                    }
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

        public Task<bool> WriteResponseAsync(IHttpResponse response, CancellationToken cancellationToken)
        {
            // add to response queue

            return Task.FromResult(true);
        }
    }
}
