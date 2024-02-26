using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi
{
    public class CgiHeaders : HttpHeaders
    {
        public CgiHeaders()
        {
        }

        public CgiHeaders(IEnumerable<HttpHeader> headers) : base(headers)
        {
        }

        public CgiHeaders(string name, string value) : base(name, value)
        {
        }

        public CgiHeaders(HttpHeader header, params HttpHeader[] headers) : base(header, headers)
        {
        }
    }
}
