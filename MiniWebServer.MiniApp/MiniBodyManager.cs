using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public class MiniBodyManager : IMiniBodyManager
    {
        private readonly PipeReader? reader;
        public MiniBodyManager(PipeReader? reader)
        {
            this.reader = reader;
        }

        public PipeReader? GetReader()
        {
            return reader;
        }
    }
}
