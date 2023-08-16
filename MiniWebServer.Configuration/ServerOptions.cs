using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Configuration
{
    public class ServerOptions
    {
        public HostOptions[] HostOptions { get; set; } = Array.Empty<HostOptions>();
        public BindingOptions[] BindingOptions { get; set; } = Array.Empty<BindingOptions>();
        public ServerFeatureOptions FeatureOptions { get; set; } = new();
    }
}
