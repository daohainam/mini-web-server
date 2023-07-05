using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Content
{
    public record class FileContentRange
    {
        public FileContentRange(long firstBytePosInclusive, long? lastBytePosInclusive)
        {
            FirstBytePosInclusive = firstBytePosInclusive;
            LastBytePosInclusive = lastBytePosInclusive;
        }

        public long FirstBytePosInclusive { get; set; }
        public long? LastBytePosInclusive { get; set; }
    }
}
