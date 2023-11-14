using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.Configuration;
using MiniWebServer.MiniApp;
using MiniWebServer.MiniWebServer.MimeMapping;
using MiniWebServer.Server.Abstractions;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace MiniWebServer.Server
{
    public class MiniWebServerBuilder : IServerBuilder
    {
        private readonly List<MiniWebServerBindingConfiguration> bindings = new();
        private readonly Dictionary<string, HostConfiguration> hosts = new();
        private readonly List<IMimeTypeMapping> mimeTypeMappings = new();
        private int connectionTimeout;
        private int readBufferSize;
        private long maxRequestBodySize;
        private int readRequestTimeout;
        private int sendResponseTimeout;

        public MiniWebServerBuilder()
        {
            Services = new ServiceCollection();
        }

        public IServiceCollection Services { get; }

        public IServerBuilder UseOptions(ServerOptions serverOptions)
        {
            if (serverOptions == null)
            {
                throw new ArgumentNullException(nameof(serverOptions));
            }

            if (serverOptions.BindingOptions.Any())
            {
                foreach (var bindingOptions in serverOptions.BindingOptions)
                {
                    if (bindingOptions.Enable && bindingOptions.Port > 0 && !string.IsNullOrEmpty(bindingOptions.Address))
                    {
                        if (bindingOptions.SSL && !string.IsNullOrEmpty(bindingOptions.Certificate))
                        {
                            // note that a certificate might have empty password
                            BindToHttps(bindingOptions.Address, bindingOptions.Port, bindingOptions.Certificate, bindingOptions.CertificatePrivateKey, bindingOptions.CertificatePassword);
                        }
                        else
                        {
                            BindToHttp(bindingOptions.Address, bindingOptions.Port);
                        }
                    }
                }
                if (serverOptions.FeatureOptions != null)
                {
                    SetConnectionTimeout(serverOptions.FeatureOptions.ConnectionTimeout);
                    SetSendResponseTimeout(serverOptions.FeatureOptions.SendResponseTimeout);
                    SetReadRequestTimeout(serverOptions.FeatureOptions.ReadRequestTimeout);
                    SetReadBufferSize(serverOptions.FeatureOptions.ReadBufferSize);
                    SetMaxRequestBodySize(serverOptions.FeatureOptions.MaxRequestBodySize);
                }
            }

            return this;
        }

        public IServerBuilder SetMaxRequestBodySize(long maxRequestBodySize)
        {
            this.maxRequestBodySize = maxRequestBodySize;

            return this;
        }

        public IServerBuilder SetReadBufferSize(int readBufferSize)
        {
            this.readBufferSize = readBufferSize;

            return this;
        }

        public IServerBuilder SetReadRequestTimeout(int readRequestTimeout)
        {
            this.readRequestTimeout = readRequestTimeout;

            return this;
        }

        public IServerBuilder SetSendResponseTimeout(int sendResponseTimeout)
        {
            this.sendResponseTimeout = sendResponseTimeout;

            return this;
        }

        public IServerBuilder SetConnectionTimeout(int connectionTimeout)
        {
            this.connectionTimeout = connectionTimeout;

            return this;
        }

        public IServerBuilder BindToHttp(string address, int port)
        {
            if (!IPAddress.TryParse(address, out IPAddress? ip))
                throw new ArgumentException(null, nameof(address));

            bindings.Add(new(
                    new IPEndPoint(ip, port)
                ));

            return this;
        }

        public IServerBuilder BindToHttps(string address, int port, string certificate, string certificatePrivateKey, string certificatePassword)
        {
            if (!IPAddress.TryParse(address, out IPAddress? ip))
                throw new ArgumentException(null, nameof(address));

            if (File.Exists(certificate))
            {
                var cert = ".pem".Equals(Path.GetExtension(certificate)) ? GenerateCertOnWindows(X509Certificate2.CreateFromPemFile(certificate, certificatePrivateKey)) : new X509Certificate2(certificate, certificatePassword);

                bindings.Add(new(
                        new IPEndPoint(ip, port), cert
                    ));

                return this;
            }
            else
            {
                throw new FileNotFoundException(certificate);
            }

            static X509Certificate2 GenerateCertOnWindows(X509Certificate2 cert) // this function works as a workaround for a bug on Windows (https://github.com/dotnet/runtime/issues/23749)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return new X509Certificate2(
                         cert.Export(
                            X509ContentType.Pkcs12
                         ));
                }
                else
                {
                    return cert;
                }
            }
        }

        public IServerBuilder AddHost(string hostName, IMiniApp app)
        {
            hosts[hostName] = new HostConfiguration(hostName, app);

            return this;
        }

        public IServerBuilder AddMimeTypeMapping(IMimeTypeMapping mimeTypeMapping)
        {
            mimeTypeMappings.Add(mimeTypeMapping);

            return this;
        }

        public IServer Build()
        {
            var serviceProvider = Services.BuildServiceProvider();

            Validate();
            AddDefaultValuesIfRequired(serviceProvider);

            Dictionary<string, Host.Host> hostContainers = new();
            foreach (var host in hosts.Values)
            {
                hostContainers.Add(host.HostName, new Host.Host(host.HostName, host.App));
            }

            var server = new MiniWebServer(new MiniWebServerConfiguration()
            {
                Bindings = bindings,
                Hosts = hosts.Values.ToList(),
                MaxRequestBodySize = maxRequestBodySize,
                ConnectionTimeout = connectionTimeout,
                ReadBufferSize = readBufferSize,
                ReadRequestTimeout = readRequestTimeout,
                SendResponseTimeout = sendResponseTimeout,
            },
                serviceProvider,
                serviceProvider.GetService<IProtocolHandlerFactory>(),
                hostContainers
            );

            return server;
        }

        private void AddDefaultValuesIfRequired(ServiceProvider serviceProvider)
        {
            if (!hosts.Any())
            {
                hosts.Add(string.Empty, new HostConfiguration(string.Empty, new DefaultMiniApp(serviceProvider)));
            }

            if (!mimeTypeMappings.Any()) // we always need a mime type mapping
            {
                mimeTypeMappings.Add(StaticMimeMapping.Instance);
            }
        }

        private void Validate() // move validating routines here so we can make Build function cleaner 
        {
            if (!bindings.Any())
            {
                throw new InvalidOperationException("No bindings found");
            }
        }
    }
}
