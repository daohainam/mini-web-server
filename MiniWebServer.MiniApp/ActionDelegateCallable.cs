using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    internal class ActionDelegateCallable : BaseCallable
    {
        private readonly ActionDelegate action;

        public ActionDelegateCallable(ActionDelegate action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public override Task InvokeAsync(IMiniAppContext context, CancellationToken cancellationToken = default)
        {
            return action.RequestDelegate.InvokeAsync(context, cancellationToken);
        }
    }
}
