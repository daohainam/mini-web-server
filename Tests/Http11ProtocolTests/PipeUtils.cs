using System.Buffers;
using System.IO.Pipelines;

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
