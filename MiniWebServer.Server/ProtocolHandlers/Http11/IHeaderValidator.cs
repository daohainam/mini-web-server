using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    public interface IHeaderValidator
    {
        bool Validate(string name, string value);
    }
}
