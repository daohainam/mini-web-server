using Microsoft.Extensions.Caching.Distributed;
using MiniWebServer.Configuration;
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
        IServerBuilder UseCache(IDistributedCache cache);
        IServerBuilder UseThreadPoolSize(int size);
        IServerBuilder UseOptions(ServerOptions serverOptions);

        /// <summary>
        /// Build a new IServer instance, it will throw an exception if an invalid options found. 
        /// If no AddRooot or AddHost with HostName = "", a default root (points to wwwroot) will be added automatically to serve as server root directory
        /// </summary>
        /// <returns></returns>
        IServer Build();
    }
}
