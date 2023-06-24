using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniApp.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniAppContext : IMiniAppContext
    {
        public MiniAppContext(MiniAppConnectionContext connectionContext, IMiniApp app, IHttpRequest request, IHttpResponse response, ISession session, IPrincipal? user)
        {
            ConnectionContext = connectionContext ?? throw new ArgumentNullException(nameof(connectionContext));
            App = app ?? throw new ArgumentNullException(nameof(app));
            Request = request ?? throw new ArgumentNullException(nameof(request));
            Response = response ?? throw new ArgumentNullException(nameof(response));
            Session = session;
            User = user;
        }

        private MiniAppConnectionContext ConnectionContext { get; }

        public IMiniApp App { get; }
        public IHttpRequest Request { get; }
        public IHttpResponse Response { get; }
        public ISession Session { get; set; } // session can be changed by session middleware
        public IServiceProvider Services => ConnectionContext.Services;
        public IPrincipal? User { get; set; }
    }
}
