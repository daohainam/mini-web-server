using MiniWebServer.Server.Abstractions;
using System.Net.Sockets;

namespace MiniWebServer.Server
{
    public class MiniWebConnectionConfiguration
    {

        public MiniWebConnectionConfiguration(
            ulong id,
            TcpClient tcpClient,
            Stream clientStream,
            bool isHttps,
            IProtocolHandler protocolHandler,
            IDictionary<string, Host.Host> hostContainers,
            IRequestIdManager requestIdManager,
            TimeSpan readRequestTimeout,
            TimeSpan sendResponseTimeout,
            TimeSpan executeTimeout,
            int readRequestBufferSize
            )
        {
            Id = id;
            TcpClient = tcpClient;
            ClientStream = clientStream ?? throw new ArgumentNullException(nameof(clientStream));
            IsHttps = isHttps;
            ProtocolHandler = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
            HostContainers = hostContainers ?? throw new ArgumentNullException(nameof(hostContainers));
            RequestIdManager = requestIdManager ?? throw new ArgumentNullException(nameof(requestIdManager));
            ReadRequestTimeout = readRequestTimeout;
            SendResponseTimeout = sendResponseTimeout;
            ExecuteTimeout = executeTimeout;
            ReadRequestBufferSize = readRequestBufferSize;
        }

        public ulong Id { get; }
        public TcpClient TcpClient { get; }
        public Stream ClientStream { get; }
        public bool IsHttps { get; }
        public IProtocolHandler ProtocolHandler { get; }
        public IDictionary<string, Host.Host> HostContainers { get; }
        public IRequestIdManager RequestIdManager { get; }
        public TimeSpan ReadRequestTimeout { get; }
        public TimeSpan SendResponseTimeout { get; }
        public TimeSpan ExecuteTimeout { get; }
        public int ReadRequestBufferSize { get; }
    }
}
