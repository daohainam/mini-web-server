using MiniWebServer.MiniApp;

namespace MiniWebServer.Mvc.Abstraction;

public class ControllerContext(IMiniAppRequestContext context, IViewEngine viewEngine)
{
    public IMiniAppRequestContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
    public IViewEngine ViewEngine { get; } = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
}
