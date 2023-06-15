using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    internal class RequestIdManager: IRequestIdManager
    {
        private ulong currentId = 1;
        public ulong GetNext()
        {
            return Interlocked.Increment(ref currentId); // this is a thread-safe function so we cannot use currentId+++
        }
    }
}
