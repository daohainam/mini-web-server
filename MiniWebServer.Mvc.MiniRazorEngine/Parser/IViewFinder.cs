using MiniWebServer.Mvc.Abstraction;

namespace MiniWebServer.Mvc.MiniRazorEngine.Parser
{
    public interface IViewFinder
    {
        string? Find(ActionResultContext context, string viewName);
    }
}
