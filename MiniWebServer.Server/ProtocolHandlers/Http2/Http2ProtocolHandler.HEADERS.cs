using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public partial class Http2ProtocolHandler
    {

        private bool ProcessHEADERSFrame(ref Http2Frame frame, ref System.Buffers.ReadOnlySequence<byte> payload, ILogger logger)
        {
            if (!Http2FrameReader.TryReadHEADERSFramePayload(ref frame, payload, headerTable, out var headersPayload, logger))
            {
                logger.LogError("Error reading HEADERS payload");
                return false;
            }

            if (inputStreamContainer.TryGetValue(frame.StreamIdentifier, out var stream))
            {
                stream.HeaderPayloads.Add(headersPayload);
                stream.FrameQueue.Add(frame);
            }
            else
            {
                // open a new stream
                stream = new Http2Stream() { 
                    StreamId = frame.StreamIdentifier,
                    FrameQueue = new StreamFrameQueue()
                };

                stream.HeaderPayloads.Add(headersPayload);

                stream.FrameQueue.Add(frame);
                inputStreamContainer[frame.StreamIdentifier] = stream;

#if DEBUG
                if (logger.IsEnabled(LogLevel.Debug)) {
                    logger.LogDebug("Stream {id} opened", frame.StreamIdentifier);
                }
#endif
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
            }

            return true;
        }
    }
}
