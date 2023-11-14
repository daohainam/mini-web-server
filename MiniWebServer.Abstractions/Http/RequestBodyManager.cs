using System.IO.Pipelines;

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
