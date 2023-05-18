using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Host
{
    public class Host
    {
        public Host(string hostName, IMiniApp app)
        {
            HostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
            App = app ?? throw new ArgumentNullException(nameof(app));
        }

        public string HostName { get; }
        public IMiniApp App { get; }
    }
}
