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
        private readonly ICallable? parent;

        public ActionDelegateCallable(ActionDelegate action, ICallable? parent = default)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.parent = parent;
        }

        public override async Task InvokeAsync(IMiniAppContext context, CancellationToken cancellationToken = default)
        {
            await action.RequestDelegate.InvokeAsync(context, cancellationToken);

            // this is an endpoint so we don't call it's parent
            //if (parent != null)
            //{
            //    await parent.InvokeAsync(context, cancellationToken);
            //}
        }
    }
}
