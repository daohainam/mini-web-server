using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.LocalAction
{
    // this class contains a list of local actions, the list is fulfilled by scanning action methods from Controller classes
    internal class LocalActionRegistry : IActionFinder, IActionRegistry
    {
        public IDictionary<string, LocalAction> Actions { get; } = new Dictionary<string, LocalAction>();

        public ActionInfo? Find(IMiniAppContext context)
        {
            if (Actions.TryGetValue(context.Request.Url, out LocalAction? localAction)) {
                return localAction.ActionInfo;
            }

            return null;
        }

        public void Register(string route, LocalAction action) // warn: this method is not thread-safe and will be called during MVC init process
        {
            Actions.Add(route, action);
        }
    }
}
