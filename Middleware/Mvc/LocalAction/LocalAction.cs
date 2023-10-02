using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.LocalAction
{
    internal class LocalAction
    {
        public LocalAction(string route, ActionInfo actionInfo)
        {
            Route = route ?? throw new ArgumentNullException(nameof(route));
            ActionInfo = actionInfo ?? throw new ArgumentNullException(nameof(actionInfo));
        }

        public string Route { get; }
        public ActionInfo ActionInfo { get; }
    }
}
