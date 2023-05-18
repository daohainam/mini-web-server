using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniAppRequest
    {
        IAppContext Context { get; }
        Cookie[] Cookies { get; }
        string Url { get; }
    }
}
