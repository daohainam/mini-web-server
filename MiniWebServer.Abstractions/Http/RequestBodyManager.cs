using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class RequestBodyManager : IRequestBodyManager
    {
        private readonly PipeReader? reader;
        public RequestBodyManager(PipeReader? reader)
        {
            this.reader = reader;
        }

        public PipeReader? GetReader()
        {
            return reader;
        }
    }
}
