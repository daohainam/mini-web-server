using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Server.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server
{
    public class MiniWebClientConnection
    {
        public enum States
        {
            Pending,
            BuildingRequestObject,
            RequestObjectReady,
            CallingResource,
            CallingResourceReady,
            ResponseObjectReady,
            ReadyToClose
        }

        public MiniWebClientConnection(int id, TcpClient tcpClient, IProtocolHandler connectionHandler, States initState)
        {
            Id = id;
            TcpClient = tcpClient;
            this.State = initState;
            ProtocolHandler = connectionHandler;
            ProtocolHandlerData = new ProtocolHandlerData();
            RequestObjectBuilder = new HttpWebRequestBuilder();
            ResponseObjectBuilder = new HttpWebResponseBuilder();
        }

        public int Id { get; }
        public TcpClient TcpClient { get; }
        public IProtocolHandler ProtocolHandler { get; }
        public ProtocolHandlerData ProtocolHandlerData { get; }
        public States State { get; set; }
        public IHttpRequestBuilder RequestObjectBuilder { get; }
        public IHttpResponseBuilder ResponseObjectBuilder { get; }
    }
}
