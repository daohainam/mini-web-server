using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniAppContext
    {
        IHttpRequest Request { get; }
        IHttpResponse Response { get; }
        ISession Session { get; set; }
        IServiceProvider Services { get; }
    }
}
