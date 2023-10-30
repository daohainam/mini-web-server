using MiniWebServer.Abstractions.Http;

namespace MiniWebServer.Abstractions
{
    public interface IParametersContainer
    {
        HttpParameters QueryParameters { get; }
    }
}