using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniApp: ICallableService
    {
        void Map(string route, RequestDelegate action, params Abstractions.Http.HttpMethod[] methods);
        void MapGet(string route, RequestDelegate action);
        void MapPost(string route, RequestDelegate action);
    }

    public delegate Task RequestDelegate(IMiniAppContext context, CancellationToken cancellationToken);
}
