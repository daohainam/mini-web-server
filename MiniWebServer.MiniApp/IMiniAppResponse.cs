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
        void SetStatus(int statusCode, string statusText);
        void SetStatus(int statusCode);
    }
}
