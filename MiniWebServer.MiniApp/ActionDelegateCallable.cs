using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    internal class ActionDelegateCallable : ICallable
    {
        private readonly ActionDelegate action;

        public ActionDelegateCallable(ActionDelegate action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public async Task Get(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken)
        {
            await action.RequestDelegate.Invoke(request, response, cancellationToken);
        }

        public async Task Post(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken)
        {
            await action.RequestDelegate.Invoke(request, response, cancellationToken);
        }
    }
}
