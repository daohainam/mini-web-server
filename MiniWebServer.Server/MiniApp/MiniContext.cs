using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniContext : IAppContext
    {
        public MiniContext(IMiniApp app)
        {
            App = app ?? throw new ArgumentNullException(nameof(app));
        }

        public IMiniApp App { get; }
    }
}
