namespace MiniWebServer.Mvc.LocalAction
{
    // this class contains a list of local actions, the list is fulfilled by scanning action methods from Controller classes
    internal class LocalActionRegistry : IActionRegistry
    {
        public IDictionary<string, LocalAction> Actions { get; } = new Dictionary<string, LocalAction>();

        public void Register(string route, LocalAction action) // warn: this method is not thread-safe and will be called during MVC init process
        {
            Actions.Add(route, action);
        }
    }
}
