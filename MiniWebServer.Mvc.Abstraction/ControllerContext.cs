using MiniWebServer.MiniApp;

namespace MiniWebServer.Mvc.Abstraction
{
    public class ControllerContext
    {
        public ControllerContext(IMiniAppContext context, IViewEngine viewEngine)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ViewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        }

        public IMiniAppContext Context { get; }
        public IViewEngine ViewEngine { get; }
    }
}