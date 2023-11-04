using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    internal class RequestDelegateAsyncCallableFilter : ICallableFilter
    {
        private readonly Func<IMiniAppContext, CancellationToken, Task<bool>> filter;

        public RequestDelegateAsyncCallableFilter(Func<IMiniAppContext, CancellationToken, Task<bool>> filter)
        {
            this.filter = filter;
        }

        public async Task<bool> InvokeAsync(IMiniAppContext context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return await filter(context, cancellationToken);
        }
    }
}
