using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.HttpsRedirection
{
    public class HttpsRedirectionOptions
    {
        public int HttpsPort { get; set; } = 443;
        public HttpResponseCodes HttpResponseCode { get; set; } = HttpResponseCodes.TemporaryRedirect;
    }
}
