using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public abstract class BaseCallable : ICallable
    {
        public virtual Task InvokeAsync(IMiniAppContext context, CancellationToken cancellationToken = default)
        {
            // by default we send 404 Not Found
            context.Response.SetStatus(Abstractions.HttpResponseCodes.NotFound);

            return Task.CompletedTask;
        }
    }
}
