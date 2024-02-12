using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi.Parsers
{
    internal class CgiResponseResponseHeader
    {
        public CgiResponseTypes CgiResponseType { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
}
