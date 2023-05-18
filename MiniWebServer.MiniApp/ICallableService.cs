using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface ICallableService
    {
        bool IsGetSupported { get; }
        ICallable? Find(IMiniAppRequest request);
    }
}
