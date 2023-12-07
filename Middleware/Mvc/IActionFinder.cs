using MiniWebServer.MiniApp;
using MiniWebServer.Mvc.Abstraction;

namespace MiniWebServer.Mvc
{
    public interface IActionFinder
    {
        ActionInfo? Find(IMiniAppRequestContext context);
    }
}
