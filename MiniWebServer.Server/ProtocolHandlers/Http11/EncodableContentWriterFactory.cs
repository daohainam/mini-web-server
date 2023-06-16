using MiniWebServer.Abstractions;
using MiniWebServer.Server.ProtocolHandlers.Http11.ContentEncoding;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    internal class EncodableContentWriterFactory
    {
        private static readonly Dictionary<string, IEncodableContentWriterCreator> creators = new(); 
        //{
        //    { "gzip", new GzipContentWriterCreator() }
        //};

        public static IContentWriter? CreateWriter(string? contentEncoding, PipeWriter parentWriter)
        {
            if (contentEncoding != null && creators.TryGetValue(contentEncoding, out IEncodableContentWriterCreator? creator))
            {
                return creator.Create(parentWriter);
            }

            return null;
        }
    }
}
