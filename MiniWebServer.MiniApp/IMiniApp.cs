using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniApp: ICallableService
    {
        Task Get(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken);
        void MapGet(string route, RequestDelegate action);
        Task Post(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken);
    }

    public delegate Task RequestDelegate(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken);
}
