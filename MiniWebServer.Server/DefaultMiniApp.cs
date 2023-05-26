using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    internal class DefaultMiniApp : BaseMiniApp
    {
        public override async Task Get(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken)
        {          
            await Task.Run(() => { response.SetStatus(HttpResponseCodes.NotFound); }, cancellationToken);
        }

        public override async Task Post(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken)
        {
            await Task.Run(() => { response.SetStatus(HttpResponseCodes.NotFound); }, cancellationToken);
        }
    }
}
