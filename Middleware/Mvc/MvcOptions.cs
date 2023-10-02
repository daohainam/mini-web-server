using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc
{
    public class MvcOptions
    {
        public MvcOptions(IActionFinder actionFinder, IRouteMatcher routeMatcher)
        {
            ActionFinder = actionFinder ?? throw new ArgumentNullException(nameof(actionFinder));
            RouteMatcher = routeMatcher ?? throw new ArgumentNullException(nameof(routeMatcher));
        }

        public IActionFinder ActionFinder { get; }
        public IRouteMatcher RouteMatcher { get; }
    }
}
