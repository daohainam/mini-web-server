using System;
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

        public override int CopyTo(Span<byte> buffer, int contentIndex)
        {
            if (contentIndex >= content.Length)
                return 0; // no data copied

            int length = Math.Min(buffer.Length, content.Length - contentIndex);
            content.AsSpan().Slice(contentIndex, length).CopyTo(buffer);

            return length;
        }
    }
}
