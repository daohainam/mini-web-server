using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;

namespace MiniWebServer.Mvc.Abstraction
{
    public class ActionResultContext
    {
        public ActionResultContext(Controller controller, ActionInfo actionInfo, IMiniAppContext appContext)
        {
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            ActionInfo = actionInfo ?? throw new ArgumentNullException(nameof(actionInfo));
            AppContext = appContext ?? throw new ArgumentNullException(nameof(appContext));
        }

        public Controller Controller { get; }
        public ActionInfo ActionInfo { get; }
        public IMiniAppContext AppContext { get; }

        public IHttpRequest Request => AppContext.Request;
        public IHttpResponse Response => AppContext.Response;
    }
}
