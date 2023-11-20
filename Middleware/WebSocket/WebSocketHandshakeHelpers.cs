using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    internal static class WebSocketHandshakeHelpers
    {
        private static ReadOnlySpan<byte> SecSocketKeyMagic => "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"u8; // server uses this 'magic' id to build Sec-WebSocket-Accept response header

        public static string BuildSecWebSocketAccept(string clientNonce)
        {
            Span<byte> mergedBytes = stackalloc byte[60]; // strlen(clientNonce) + strlen(SecSocketKeyMagic) 
            Encoding.UTF8.GetBytes(clientNonce, mergedBytes);
            SecSocketKeyMagic.CopyTo(mergedBytes[24..]);
            Span<byte> sha1 = stackalloc byte[20];
            if (SHA1.HashData(mergedBytes, sha1) == 20)
            {
                throw new InvalidOperationException("Could not hash data for Sec-WebSocket-Accept header value");
            }
            else
            {
                return Convert.ToBase64String(sha1);
            }
        }
    }
}
