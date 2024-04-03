using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    public class HPACKInteger
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Decode(int v, int n)
        {
            // https://httpwg.org/specs/rfc7541.html#integer.representation
            return (int)v;
        }
    }
}
