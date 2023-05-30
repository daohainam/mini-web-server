using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniAppContext : IMiniAppContext
    {
        public MiniAppContext(MiniAppConnectionContext connectionContext, IMiniApp app, IMiniAppRequest request, IMiniAppResponse response)
        {
            ConnectionContext = connectionContext ?? throw new ArgumentNullException(nameof(connectionContext));
            App = app ?? throw new ArgumentNullException(nameof(app));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public MiniAppConnectionContext ConnectionContext { get; }

        public IMiniApp App { get; }
        public IMiniAppRequest Request { get; internal set; }
        public IMiniAppResponse Response { get; internal set; }
    }
}
