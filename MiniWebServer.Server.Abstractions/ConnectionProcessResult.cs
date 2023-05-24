using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Abstractions
{
    public class ConnectionProcessResult
    {
        public bool Success { get; }
        public bool CloseConnectionRequested { get; }
        public int ProtocolVersionRequested { get; }
    }
}
