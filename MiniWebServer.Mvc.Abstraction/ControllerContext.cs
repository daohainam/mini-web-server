using MiniWebServer.MiniApp;

namespace MiniWebServer.Mvc.Abstraction
{
    public class ControllerContext
    {
        public ControllerContext(IMiniAppRequestContext context, IViewEngine viewEngine)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ViewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        }

        public IMiniAppRequestContext Context { get; }
        public IViewEngine ViewEngine { get; }
    }
}