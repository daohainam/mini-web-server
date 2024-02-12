using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi
{
    public class CgiResponseStreamReaderOptions
    {
        public int ScriptMaxRunningTimeInMs { get; set; } = 1000 * 60 * 15; // 15 minutes
    }
}
