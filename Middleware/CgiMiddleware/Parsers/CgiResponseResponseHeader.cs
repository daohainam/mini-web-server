using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi.Parsers;

internal class CgiResponseResponseHeader
{
    public CgiResponseTypes CgiResponseType { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public HttpResponseCodes ResponseCode { get; set; }
    public string ReasonPhrase { get; set; } = string.Empty;
}
