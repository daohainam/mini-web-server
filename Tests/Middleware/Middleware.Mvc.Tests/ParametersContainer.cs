using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;

namespace MvcMiddlewareTests;

internal class ParametersContainer : IParametersContainer
{
    public required HttpParameters QueryParameters { get; init; }
}
