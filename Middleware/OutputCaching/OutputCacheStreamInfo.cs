using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.OutputCaching
{
    public class OutputCacheStreamInfo
    {
        public required IHttpContent Content { get; set; }
        public required HttpResponseCodes StatusCode { get; set; }
        public required HttpHeader Headers { get; set; }
    }
}
