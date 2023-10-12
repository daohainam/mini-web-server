using MiniWebServer.Mvc.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.MiniRazorEngine.Parser
{
    public interface IViewFinder
    {
        string? Find(ActionResultContext context, string viewName);
    }
}
