using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public class Http2Frame
    {
        /*
         * From RFC 9113:

            4.1. Frame Format
            All frames begin with a fixed 9-octet header followed by a variable-length frame payload.

            HTTP Frame {
                Length (24),
                Type (8),

                Flags (8),

                Reserved (1),
                Stream Identifier (31),

                Frame Payload (..),
            }
            Figure 1: Frame Layout
            The fields of the frame header are defined as:

            Length:
            The length of the frame payload expressed as an unsigned 24-bit integer in units of octets. Values greater than 214 (16,384) MUST NOT be sent unless the receiver has set a larger value for SETTINGS_MAX_FRAME_SIZE.

            The 9 octets of the frame header are not included in this value.

            Type:
            The 8-bit type of the frame. The frame type determines the format and semantics of the frame. Frames defined in this document are listed in Section 6. Implementations MUST ignore and discard frames of unknown types.

            Flags:
            An 8-bit field reserved for boolean flags specific to the frame type.

            Flags are assigned semantics specific to the indicated frame type. Unused flags are those that have no defined semantics for a particular frame type. Unused flags MUST be ignored on receipt and MUST be left unset (0x00) when sending.

            Reserved:
            A reserved 1-bit field. The semantics of this bit are undefined, and the bit MUST remain unset (0x00) when sending and MUST be ignored when receiving.

            Stream Identifier:
            A stream identifier (see Section 5.1.1) expressed as an unsigned 31-bit integer. The value 0x00 is reserved for frames that are associated with the connection as a whole as opposed to an individual stream.

            The structure and content of the frame payload are dependent entirely on the frame type.
             */

        public int Length { get; set; }
        public Http2FrameType FrameType { get; set; }
        public byte Flags { get; set; }
        public int StreamIdentifier { get; set; }
    }
}
