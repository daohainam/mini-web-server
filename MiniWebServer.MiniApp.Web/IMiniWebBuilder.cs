using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Web
{
    public interface IMiniWebBuilder
    {
        MiniWeb Build();
        IMiniWebBuilder UseStaticFiles();
        IMiniWebBuilder UseRootDirectory(string path);
    }
}
