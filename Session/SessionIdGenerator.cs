using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Session
{
    internal class SessionIdGenerator : ISessionIdGenerator
    {
        public const int SessionIdLength = 128;

        private const string chars = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_.!*#~";
        private static readonly Random rand = new();
        public string GenerateNewId()
        {
            var buffer = ArrayPool<char>.Shared.Rent(SessionIdLength);
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                buffer[i] = chars[rand.Next(chars.Length)];
            }

            var sessionId = new string(buffer);
            ArrayPool<char>.Shared.Return(buffer);

            return sessionId;
        }
    }
}
