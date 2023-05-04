using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    public class HostConfiguration
    {
        public HostConfiguration(string hostName, string rootDirectory)
        {
            HostName = hostName;
            RootDirectory = rootDirectory;
        }

        public string HostName { get; }
        public string RootDirectory { get; }
    }
}
