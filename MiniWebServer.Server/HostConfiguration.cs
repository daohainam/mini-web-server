using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    public class HostConfiguration
    {
        public HostConfiguration(string hostName, IMiniApp app)
        {
            HostName = hostName;
            App = app;
        }

        public string HostName { get; }
        public IMiniApp App { get; }
    }
}
