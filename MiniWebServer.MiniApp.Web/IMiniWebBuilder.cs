using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Web
{
    public interface IMiniWebBuilder: IMiniAppBuilder
    {
        IMiniWebBuilder UseStaticFiles();
        IMiniWebBuilder UseRootDirectory(string path);
    }
}
