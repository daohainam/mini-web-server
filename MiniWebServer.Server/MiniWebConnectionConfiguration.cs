using MiniWebServer.Server.Abstractions;
using System.Net.Sockets;

namespace MiniWebServer.Server
{
    public class MiniWebConnectionConfiguration(
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
        public ulong Id { get; } = id;
        public TcpClient TcpClient { get; } = tcpClient;
        public Stream ClientStream { get; } = clientStream ?? throw new ArgumentNullException(nameof(clientStream));
        public bool IsHttps { get; } = isHttps;
        public IProtocolHandler ProtocolHandler { get; } = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
        public IDictionary<string, Host.Host> HostContainers { get; } = hostContainers ?? throw new ArgumentNullException(nameof(hostContainers));
        public IRequestIdManager RequestIdManager { get; } = requestIdManager ?? throw new ArgumentNullException(nameof(requestIdManager));
        public TimeSpan ReadRequestTimeout { get; } = readRequestTimeout;
        public TimeSpan SendResponseTimeout { get; } = sendResponseTimeout;
        public TimeSpan ExecuteTimeout { get; } = executeTimeout;
        public int ReadRequestBufferSize { get; } = readRequestBufferSize;
    }
}
