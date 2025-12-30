namespace MiniWebServer.Mvc;

public class MvcOptions(IActionFinder actionFinder)
{
    public IActionFinder ActionFinder { get; } = actionFinder ?? throw new ArgumentNullException(nameof(actionFinder));
}
