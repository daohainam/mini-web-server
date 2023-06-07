using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniAppContext
    {
        IMiniAppRequest Request { get; }
        IMiniAppResponse Response { get; }
        ISession Session { get; set; }
    }
}
