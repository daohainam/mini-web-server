using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public abstract class MiniContent
    {
        public virtual IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public abstract Task<int> WriteToAsync(IBufferWriter<byte> writer, CancellationToken cancellationToken);
        public abstract long ContentLength { get; }
    }
}
