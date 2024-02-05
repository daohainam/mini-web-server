using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi
{
    public class CgiResponse
    {
        public required IHttpContent Content { get; set; }
        public required CgiHeaders Headers { get; set; }
        public required HttpResponseCodes ResponseCode { get; set; }
    }
}
