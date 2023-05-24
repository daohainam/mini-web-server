using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.Abstractions
{
    public interface IProtocolHandlerFactory
    {
        IProtocolHandler Create(ProtocolHandlerConfiguration config);
    }
}
