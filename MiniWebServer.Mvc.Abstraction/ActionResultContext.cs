using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;

namespace MiniWebServer.Mvc.Abstraction;

public class ActionResultContext(Controller controller, ActionInfo actionInfo, IMiniAppRequestContext appContext)
{
    public Controller Controller { get; } = controller ?? throw new ArgumentNullException(nameof(controller));
    public ActionInfo ActionInfo { get; } = actionInfo ?? throw new ArgumentNullException(nameof(actionInfo));
    public IMiniAppRequestContext AppContext { get; } = appContext ?? throw new ArgumentNullException(nameof(appContext));

    public IHttpRequest Request => AppContext.Request;
    public IHttpResponse Response => AppContext.Response;
}
