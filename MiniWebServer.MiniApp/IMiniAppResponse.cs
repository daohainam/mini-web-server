using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniAppResponse
    {
        void AddHeader(string name, string mimeType);
        void SetContent(MiniContent content);
        void SetStatus(HttpResponseCodes statusCode, string reasonPhrase);
        void SetStatus(HttpResponseCodes statusCode);
    }
}
