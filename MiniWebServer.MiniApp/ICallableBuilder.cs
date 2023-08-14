using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface ICallableBuilder
    {
        ICallableBuilder AddFilter(ICallableFilter filter);
        ICallableBuilder AddFilter(Func<IMiniAppContext, CancellationToken, bool> filter);
        ICallableBuilder AddFilter(Func<IMiniAppContext, CancellationToken, Task<bool>> filter);
    }
}
