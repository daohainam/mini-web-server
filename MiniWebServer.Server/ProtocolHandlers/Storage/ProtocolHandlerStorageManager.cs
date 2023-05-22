using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Storage
{
    // this a abstract factory which hides complicated things behind
    public class ProtocolHandlerStorageManager : IProtocolHandlerStorageManager
    {
        private readonly ILogger<ProtocolHandlerStorageManager> logger;

        public ProtocolHandlerStorageManager(ILogger<ProtocolHandlerStorageManager> logger) 
        { 
            this.logger = logger;
        }

        public IProtocolHandlerStorage GetStorage(long proposedSize)
        {
            return GetStorage(proposedSize, Array.Empty<TransferEncodings>());
        }
        public IProtocolHandlerStorage GetStorage(long proposedSize, TransferEncodings[] transferEncoding)
        {
            if (transferEncoding.Length == 0)
            {
                return new FileProtocolHandlerStorage(Path.Combine(Path.GetTempPath(), ".temp"), logger); // todo: we should have a better strategy here to return a secure path
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
