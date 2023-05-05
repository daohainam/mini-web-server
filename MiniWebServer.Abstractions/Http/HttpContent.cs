using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public abstract class HttpContent
    {
        public virtual IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public abstract Task WriteTo(Stream stream);
    }
}
