using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Session
{
    public class DistributedCacheSessionStoreOptions
    {
        public int LockWaitTimeoutMs { get; set; } = Timeout.Infinite;
    }
}
