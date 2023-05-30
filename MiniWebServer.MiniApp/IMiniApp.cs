using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniApp: ICallableService
    {
        Task Get(IMiniAppContext context, CancellationToken cancellationToken);
        void MapGet(string route, RequestDelegate action);
        void MapPost(string route, RequestDelegate action);
        Task Post(IMiniAppContext context, CancellationToken cancellationToken);
    }

    public delegate Task RequestDelegate(IMiniAppContext context, CancellationToken cancellationToken);
}
