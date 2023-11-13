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
        public MvcOptions(IActionFinder actionFinder)
        {
            ActionFinder = actionFinder ?? throw new ArgumentNullException(nameof(actionFinder));
        }

        public IActionFinder ActionFinder { get; }
    }
}
