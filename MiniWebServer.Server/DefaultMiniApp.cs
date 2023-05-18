using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    internal class DefaultMiniApp : IMiniApp
    {
        public async Task Get(IMiniAppRequest request, IMiniAppResponse response)
        {          
            await Task.Run(() => { response.SetStatus(HttpResponseCodes.NotFound); });
        }
    }
}
