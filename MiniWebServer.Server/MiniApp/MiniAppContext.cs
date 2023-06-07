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
        public MiniAppContext(MiniAppConnectionContext connectionContext, IMiniApp app, IMiniAppRequest request, IMiniAppResponse response, ISession session)
        {
            ConnectionContext = connectionContext ?? throw new ArgumentNullException(nameof(connectionContext));
            App = app ?? throw new ArgumentNullException(nameof(app));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
            Session = session;
        }

        public MiniAppConnectionContext ConnectionContext { get; }

        public IMiniApp App { get; }
        public IMiniAppRequest Request { get; }
        public IMiniAppResponse Response { get; }
        public ISession Session { get; set; } // session can be changed by session middleware
    }
}
