namespace MiniWebServer.Mvc.LocalAction;

internal interface IActionRegistry
{
    void Register(string route, LocalAction action);
}
