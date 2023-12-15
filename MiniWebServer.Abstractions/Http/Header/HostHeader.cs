using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http.Header
{
    public class HostHeader
    {
        public string Host { get; set; } = string.Empty; 
        public int Port { get; set; } = -1;

        public static bool TryParse(string value, out HostHeader? hostHeader)
        {
            int idx = value.IndexOf(':');
            if (idx != -1)
            {
                string host = value[..idx];
                if (int.TryParse(value[(idx + 1)..], out var port))
                {
                    if (port > 0)
                    {
                        hostHeader = new HostHeader { 
                            Host = host, 
                            Port = port 
                        };
                        return true;
                    }
                }
            }
            else
            {
                hostHeader = new HostHeader
                {
                    Host = value,
                    Port = 0 // 0 means there was no port in Host header 
                };
                return true;
            }

            hostHeader = default;
            return false;
        }
    }
}
