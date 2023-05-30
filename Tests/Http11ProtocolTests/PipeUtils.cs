using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Http11ProtocolTests
{
    internal class PipeUtils
    {
        public static PipeReader String2Reader(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return PipeReader.Create(new ReadOnlySequence<byte>(stream.GetBuffer()).Slice(0, stream.Length));
        }
    }
}
