using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniAppResponse
    {
        void AddCookie(HttpCookie cookie);
        void AddHeader(string name, string mimeType);
        void SetContent(MiniContent content);
        void SetStatus(HttpResponseCodes statusCode, string reasonPhrase);
        void SetStatus(HttpResponseCodes statusCode);
    }
}
