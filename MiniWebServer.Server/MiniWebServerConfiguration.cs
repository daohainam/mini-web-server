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
        public const int DefaultThreadPoolSize = 4;

        public IPEndPoint HttpEndPoint { get; init; } = new(IPAddress.Loopback, 80);
        public int ThreadPoolSize { get; init; } = DefaultThreadPoolSize;
        /// <summary>
        /// a connection will be closed if it has no data in ConnectionTimeout (in ms)
        /// </summary>
        public int ConnectionTimeout { get; init; } = 3000; 
        public string Root { get; init; } = string.Empty;
        public X509Certificate2? Certificate { get; internal set; }

        public List<HostConfiguration> Hosts = new();

        // advanced settings
        public int ReadBufferSize { get; init; } = 1024 * 8;
        public long MaxRequestBodySize { get; internal set; } = 1024 * 1024 * 10; // 10MB 
    }
}
