using MiniWebServer.Mvc.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc
{
    public class MvcOptions
    {
        public MvcOptions(IActionFinder actionFinder, IViewEngine viewEngine)
        {
            ActionFinder = actionFinder ?? throw new ArgumentNullException(nameof(actionFinder));
            ViewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
        }

        public IActionFinder ActionFinder { get; }
        public IViewEngine ViewEngine { get; }
    }
}
