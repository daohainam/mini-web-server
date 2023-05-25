using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    public class MiniWebServerConfiguration
    {
        public IPEndPoint HttpEndPoint { get; init; } = new(IPAddress.Loopback, 80);
        /// <summary>
        /// a connection will be closed if it has no data in ConnectionTimeout (in ms)
        /// </summary>
        public string Root { get; init; } = string.Empty;
        public X509Certificate2? Certificate { get; internal set; }

        public List<HostConfiguration> Hosts = new();

        // advanced settings
        public int ReadBufferSize { get; init; } = 1024 * 8;
        public long MaxRequestBodySize { get; set; } = 1024 * 1024 * 10; // 10MB 
        public int ReadRequestTimeout { get; set; } = 180000;
        public int SendResponseTimeout { get; set; } = 300000;
        public int ConnectionTimeout { get; set; } = 180000;
    }
}
