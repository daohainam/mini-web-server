using MiniWebServer.MiniApp;

namespace MiniWebServer.Mvc.Abstraction
{
    public class ControllerContext
    {
        public ControllerContext(IMiniAppContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IMiniAppContext Context { get; }
    }
}