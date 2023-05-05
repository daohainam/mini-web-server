using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MimeMapping;
using MiniWebServer.Abstractions;
using MiniWebServer.Server.Host;
using MiniWebServer.Server.MimeType;
using MiniWebServer.Server.Routing;
using MiniWebServer.Server.StaticFileSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    public class MiniWebServerBuilder : IServerBuilder
    {
        private IPAddress address = IPAddress.Loopback;
        private int httpPort = 80;
        private readonly ILogger<MiniWebServerBuilder> logger;
        private IDistributedCache cache;
        private readonly Dictionary<string, HostConfiguration> hosts = new();
        private readonly List<IRoutingServiceFactory> routingServiceFactories = new();
        private readonly List<IMimeTypeMapping> mimeTypeMappings = new();

        public MiniWebServerBuilder(ILogger<MiniWebServerBuilder>? logger, IDistributedCache? cache)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public IServerBuilder BindToAddress(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out IPAddress? ip))
                throw new ArgumentException(null, nameof(ipAddress));

            this.address = ip;

            return this;
        }

        public IServerBuilder UseHttpPort(int httpPort)
        {
            if (httpPort < 0 || httpPort > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(httpPort));
            } 

            this.httpPort = httpPort;

            return this;
        }

        public IServerBuilder AddRoot(string rootDirectory)
        {
            return AddHost(string.Empty, rootDirectory);
        }

        public IServerBuilder AddHost(string hostName, string hostDirectory)
        {
            hosts[hostName] = new HostConfiguration(hostName, hostDirectory);

            return this;
        }

        public IServerBuilder AddRoutingServiceFactory(IRoutingServiceFactory routingServiceFactory)
        {
            routingServiceFactories.Add(routingServiceFactory);

            return this;
        }

        public IServerBuilder AddMimeTypeMapping(IMimeTypeMapping mimeTypeMapping)
        {
            mimeTypeMappings.Add(mimeTypeMapping);

            return this;
        }

        public IServerBuilder UseCache(IDistributedCache cache)
        {
            this.cache = cache;

            return this;
        }

        public IServerBuilder UseStaticFiles()
        {
            return AddRoutingServiceFactory(
                new StaticFileRoutingServiceFactory(logger)
                );
        }

        public IServer Build()
        {
            Validate();
            AddDefaultValuesIfRequired();

            Dictionary<string, HostContainer> hostContainers = new();
            foreach (var host in hosts.Values)
            {
                var routingService = new RoutingService();
                foreach (var factory in routingServiceFactories)
                {
                    routingService.AddRoute(factory.Create(host.RootDirectory));
                }

                hostContainers.Add(host.HostName, new HostContainer(
                    host.HostName, new DirectoryInfo(host.RootDirectory), routingService)
                    ); 
            }

            var server = new MiniWebServer(new MiniWebServerConfiguration() { 
                    HttpEndPoint = new IPEndPoint(address, httpPort),
                    Hosts = hosts.Values.ToList(),
                },
                new ProtocolHandlerFactory(logger),
                hostContainers,
                new CachableMimeTypeMapping(new MimeTypeMappingContainer(mimeTypeMappings), cache),
                logger
            );

            return server;
        }

        private void AddDefaultValuesIfRequired()
        {
            if (!hosts.Any())
            {
                hosts.Add(string.Empty, new HostConfiguration(string.Empty, Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName ?? string.Empty, "wwwroot")));
            }

            if (!mimeTypeMappings.Any()) // we always need a mime type mapping
            {
                mimeTypeMappings.Add(StaticMimeMapping.GetInstance());
            }
        }

        private void Validate() // move validating routines here so we can make Build function shorter 
        {
            if (cache == null) 
            {
                // Opps, this will never happen
                throw new InvalidOperationException("Cache required");
            }
        }
    }
}
