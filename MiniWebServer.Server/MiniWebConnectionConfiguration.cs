using MiniWebServer.Abstractions;
using MiniWebServer.Server.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    public class MiniWebConnectionConfiguration
    {

        public MiniWebConnectionConfiguration(
            int id, 
            TcpClient tcpClient, 
            Stream clientStream, 
            IProtocolHandler protocolHandler, 
            IDictionary<string, Host.Host> hostContainers,
            TimeSpan readRequestTimeout,
            TimeSpan sendResponseTimeout,
            TimeSpan executeTimeout, 
            int readRequestBufferSize
            )
        {
            Id = id;
            TcpClient = tcpClient;
            ClientStream = clientStream ?? throw new ArgumentNullException(nameof(clientStream));
            ProtocolHandler = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
            HostContainers = hostContainers ?? throw new ArgumentNullException(nameof(hostContainers));
            ReadRequestTimeout = readRequestTimeout;
            SendResponseTimeout = sendResponseTimeout;
            ExecuteTimeout = executeTimeout;
            ReadRequestBufferSize = readRequestBufferSize;
        }

        public int Id { get; }
        public TcpClient TcpClient { get; }
        public Stream ClientStream { get; }
        public IProtocolHandler ProtocolHandler { get; }
        public IDictionary<string, Host.Host> HostContainers { get; }
        public TimeSpan ReadRequestTimeout { get; }
        public TimeSpan SendResponseTimeout { get; }
        public TimeSpan ExecuteTimeout { get; }
        public int ReadRequestBufferSize { get; }
    }
}
