using MiniWebServer.Mvc.Abstraction;

namespace MiniWebServer.Mvc.LocalAction
{
    internal class LocalAction
    {
        public LocalAction(string route, ActionInfo actionInfo, ActionMethods actionMethods = ActionMethods.All)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            ActionInfo = actionInfo ?? throw new ArgumentNullException(nameof(actionInfo));
            ActionMethods = actionMethods;
        }

        public string Route { get; }
        public ActionInfo ActionInfo { get; }
        public ActionMethods ActionMethods { get; }
    }
}
