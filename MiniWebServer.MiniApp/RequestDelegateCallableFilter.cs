using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    internal class RequestDelegateCallableFilter : ICallableFilter
    {
        private Func<IMiniAppContext, CancellationToken, bool> filter;

        public RequestDelegateCallableFilter(Func<IMiniAppContext, CancellationToken, bool> filter)
        {
            this.filter = filter;
        }

        public async Task<bool> InvokeAsync(IMiniAppContext context, CancellationToken cancellationToken)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return await Task.FromResult(filter(context, cancellationToken));
        }
    }
}
