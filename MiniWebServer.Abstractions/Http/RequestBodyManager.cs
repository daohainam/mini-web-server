using System.IO.Pipelines;

namespace MiniWebServer.Abstractions.Http;

public class RequestBodyManager(PipeReader? reader) : IRequestBodyManager
{
    public PipeReader? GetReader()
    {
        return reader;
    }
}
