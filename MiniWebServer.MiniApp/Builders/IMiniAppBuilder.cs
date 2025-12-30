using Microsoft.Extensions.DependencyInjection;

namespace MiniWebServer.MiniApp.Builders;

public interface IMiniAppBuilder
{
    IMiniApp Build();
    IServiceCollection Services { get; }
    IMiniAppBuilder UseMiddleware(Type middlewareType);
    IMiniAppBuilder UseMiddleware<TMiddleware>();
}
