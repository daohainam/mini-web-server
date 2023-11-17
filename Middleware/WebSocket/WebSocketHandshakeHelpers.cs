using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket
{
    internal class WebSocketHandshakeHelpers
    {
        private const string SecSocketKeyMagic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"; // server uses this 'magic' id to build Sec-WebSocket-Accept response header

        public static string BuildSecWebSocketAccept(string clientNonce)
        {
            StringBuilder sb = new();

            sb.Append(clientNonce);
            sb.Append(SecSocketKeyMagic);

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var sha1 = SHA1.HashData(bytes);

            return Convert.ToBase64String(sha1);
        }
    }
}
