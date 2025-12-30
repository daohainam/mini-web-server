using MiniWebServer.Mvc.Abstraction;

namespace MiniWebServer.Mvc.LocalAction;

internal class LocalAction(string route, ActionInfo actionInfo, ActionMethods actionMethods = ActionMethods.All)
{
    public string Route { get; } = route ?? throw new ArgumentNullException(nameof(route));
    public ActionInfo ActionInfo { get; } = actionInfo ?? throw new ArgumentNullException(nameof(actionInfo));
    public ActionMethods ActionMethods { get; } = actionMethods;
}
