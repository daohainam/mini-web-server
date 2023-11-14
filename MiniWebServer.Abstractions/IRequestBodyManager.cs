using System.IO.Pipelines;

namespace MiniWebServer.Abstractions
{
    public interface IRequestBodyManager
    {
        PipeReader? GetReader();
    }
}
