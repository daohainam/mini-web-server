using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.Configuration;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Abstractions
{
    public interface IServerBuilder
    {
        IServiceCollection Services { get; }
        IServerBuilder BindToHttp(string ipAddress, int port);
        IServerBuilder BindToHttps(string address, int port, string certificate, string certificatePrivateKey, string certificatePassword);
        IServerBuilder AddHost(string hostName, IMiniApp host);
        IServerBuilder UseOptions(ServerOptions serverOptions);
        IServerBuilder SetMaxRequestBodySize(long maxRequestBodySize);
        IServerBuilder SetReadBufferSize(int readBufferSize);
        IServerBuilder SetReadRequestTimeout(int readRequestTimeout);
        IServerBuilder SetSendResponseTimeout(int sendResponseTimeout);
        IServerBuilder SetConnectionTimeout(int connectionTimeout);

        /// <summary>
        /// Build a new IServer instance, it will throw an exception if an invalid options found. 
        /// If no AddRooot or AddHost with HostName = "", a default root (points to wwwroot) will be added automatically to serve as server root directory
        /// </summary>
        /// <returns></returns>
        IServer Build();
    }
}
