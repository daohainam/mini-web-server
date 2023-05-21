using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface IMiniApp
    {
        Task Get(IMiniAppRequest request, IMiniAppResponse response, CancellationToken cancellationToken);
    }
}
