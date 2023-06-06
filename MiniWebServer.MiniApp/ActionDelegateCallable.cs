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

        public override async Task Get(IMiniAppContext context, CancellationToken cancellationToken)
        {
            await action.RequestDelegate.Invoke(context, cancellationToken);
        }

        public override async Task Post(IMiniAppContext context, CancellationToken cancellationToken)
        {
            await action.RequestDelegate.Invoke(context, cancellationToken);
        }
    }
}
