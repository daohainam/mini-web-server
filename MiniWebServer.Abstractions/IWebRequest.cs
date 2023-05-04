using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    public interface IWebRequest
    {
        HttpMethod Method { get; }
        string Url { get; }
        IReadOnlyDictionary<string, string> Headers { get; }
    }
}
