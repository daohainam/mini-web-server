using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Content
{
    public class EmptyBodyFileContent : FileContent
    {
        public EmptyBodyFileContent(string fileName): base(fileName)
        {
        }
        public EmptyBodyFileContent(FileInfo file): base(file)
        {
        }

        public override Task<long> WriteToAsync(IContentWriter writer, CancellationToken cancellationToken)
        {
            // we send nothing, this response content is used mainly to serve HEAD requests

            return Task.FromResult(0L);
        }
    }
}
