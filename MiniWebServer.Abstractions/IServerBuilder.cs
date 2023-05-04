using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    public interface IServerBuilder
    {
        IServerBuilder UseHttpPort(int httpPort);
        IServerBuilder BindToAddress(string ipAddress);
        IServerBuilder AddRoot(string rootDirectory);
        IServerBuilder AddHost(string hostName, string hostDirectory);
        IServerBuilder UseStaticFiles();
        IServer Build();
    }
}
