using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Content
{
    public class ByteArrayContent : MiniContent
    {
        private readonly byte[] content;

        public ByteArrayContent(byte[] content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public override long ContentLength => content.LongLength;

        public override async Task<int> WriteToAsync(IBufferWriter<byte> writer, CancellationToken cancellationToken)
        {
            writer.Write(content);

            return await Task.FromResult(content.Length);
        }
    }
}
