using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class StreamFrameQueue
    {
        private readonly Queue<Http2Frame> frames = [];
        private SpinLock spinLock = new();

        public IEnumerable<Http2Frame> Frames => frames;
        public DateTime LastModifyTime { get; private set; }

        public bool Add(Http2Frame frame)
        {
            var gotLock = false;
            try
            {
                spinLock.Enter(ref gotLock);
                frames.Enqueue(frame);
                LastModifyTime = DateTime.Now;
            }
            finally
            {
                if (gotLock) spinLock.Exit();
            }

            return gotLock;
        }
    }
}
