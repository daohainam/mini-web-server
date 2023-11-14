namespace MiniWebServer.Mvc
{
    public class MvcOptions
    {
        public MvcOptions(IActionFinder actionFinder)
        {
            ActionFinder = actionFinder ?? throw new ArgumentNullException(nameof(actionFinder));
        }

        public IActionFinder ActionFinder { get; }
    }
}
