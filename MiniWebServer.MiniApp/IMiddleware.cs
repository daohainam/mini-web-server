using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiddleware
    {
        Task InvokeAsync(IMiniAppContext context, ICallable next, CancellationToken cancellationToken = default);
    }
}
