using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    public class MiniWebServerConfiguration
    {
        public const int DefaultThreadPoolSize = 4;

        public IPEndPoint HttpEndPoint { get; init; } = new(IPAddress.Loopback, 80);
        public int ThreadPoolSize { get; init; } = DefaultThreadPoolSize;
        public string Root { get; init; } = string.Empty;
        public List<HostConfiguration> Hosts = new List<HostConfiguration>();
    }
}
