using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    public interface IProtocolHandlerStorageManager
    {
        IProtocolHandlerStorage GetStorage(long proposedSize);
        public IProtocolHandlerStorage GetStorage(long proposedSize, TransferEncodings[] transferEncoding);

    }
}
