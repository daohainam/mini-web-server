using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Abstractions
{
    public class ProtocolHandlerContext
    {
        public required PipeReader PipeReader { get; set; }
        public required Stream Stream { get; set; }
    }
}
