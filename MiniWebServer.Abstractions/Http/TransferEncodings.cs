using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public enum TransferEncodings
    {
        None,
        Chunked,
        Compress,
        Deflate,
        Gzip
    }
}
