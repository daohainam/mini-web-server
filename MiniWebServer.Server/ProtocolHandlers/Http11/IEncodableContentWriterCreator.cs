using MiniWebServer.Abstractions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    internal interface IEncodableContentWriterCreator
    {
        IContentWriter Create(IBufferWriter<byte> parentWriter);
    }
}
