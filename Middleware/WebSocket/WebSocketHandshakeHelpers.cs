using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.WebSocket;

internal static class WebSocketHandshakeHelpers
{
    private static ReadOnlySpan<byte> SecSocketKeyMagic => "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"u8; // server uses this 'magic' id to build Sec-WebSocket-Accept response header

    public static string BuildSecWebSocketAccept(string clientNonce)
    {
        Span<byte> mergedBytes = stackalloc byte[60]; // strlen(clientNonce) + strlen(SecSocketKeyMagic) 
        Span<byte> sha1 = stackalloc byte[20];

        Encoding.UTF8.GetBytes(clientNonce, mergedBytes);
        SecSocketKeyMagic.CopyTo(mergedBytes[24..]); // clientNonce is a base64 string of a 16-byte array, so it's length is always 24 (= 16 * 8 / 6)
        if (SHA1.HashData(mergedBytes, sha1) != 20)
        {
            throw new InvalidOperationException("Could not hash data for Sec-WebSocket-Accept header value");
        }
        else
        {
            return Convert.ToBase64String(sha1);
        }
    }

    public static bool IsUpgradeRequest(IMiniAppRequestContext context, out string? originalNonce, ILogger logger)
    {
        /*
         * check if this is a upgrade request
         * an upgrade request must be a GET request and has following headers:
         * - Upgrade: websocket
         * - Connection: Upgrade
         * - Sec-WebSocket-Key: <key> // a base64 encoded string from a randomly generated 16-ASCII-character-length string
         * - Sec-WebSocket-Version: <version>
         * 
         * the request can also contains following headers:
         *  - Sec-WebSocket-Protocol: <protocol>
         *  - Sec-WebSocket-Accept
         */

        // TODO: to prevent DDOS, we should handle re-handshakes, and should not allow multiple handshakes from same IP
        if (context.Request.Method == MiniWebServer.Abstractions.Http.HttpMethod.Get)
        {
            if ("websocket".Equals(context.Request.Headers.Upgrade, StringComparison.OrdinalIgnoreCase)
                && "Upgrade".Equals(context.Request.Headers.Connection, StringComparison.OrdinalIgnoreCase)
                && "13".Equals(context.Request.Headers.SecWebSocketVersion) // current version of WebSocket protocol, I don't think they will have a newer one soon
                )
            {
                var secWebSocketKey = context.Request.Headers.SecWebSocketKey;
                if (!string.IsNullOrWhiteSpace(secWebSocketKey))
                {
                    try
                    {
                        /*
                        var bytes = Convert.FromBase64String(secWebSocketKey);
                        if (bytes.Length != 16)
                        {
                            throw new FormatException("Sec-WebSocket-Key original value length must be 16");
                        }
                        */
                        originalNonce = secWebSocketKey;
                    }
                    catch
                    {
                        logger.LogError("Invalid Sec-WebSocket-Key value: {v}", secWebSocketKey);
                        throw;
                    }

                    return originalNonce != null;
                }
            }
        }

        originalNonce = null;
        return false;
    }
}
