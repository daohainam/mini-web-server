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
        private bool ProcessHEADERSFrame(ref Http2Frame frame, ref System.Buffers.ReadOnlySequence<byte> payload)
        {
            if (!Http2FrameReader.TryReadHEADERSFramePayload(ref frame, ref payload, out var headersPayload))
            {
                logger.LogError("Error reading WINDOW_UPDATE payload");
                return false;
            }

            if (streamContainer.ContainsKey(frame.StreamIdentifier))
            {

            }
            else
            {
                // open a new stream
                var stream = new Http2Stream() { 
                    StreamId = frame.StreamIdentifier,
                    FrameQueue = new StreamFrameQueue()
                };

                stream.FrameQueue.Add(frame);
                streamContainer[frame.StreamIdentifier] = stream;

                if (logger.IsEnabled(LogLevel.Debug)) {
                    logger.LogDebug("Stream {id} opened", frame.StreamIdentifier);
                }
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
            }

            return true;
        }
    }
}
