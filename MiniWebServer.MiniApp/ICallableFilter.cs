using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public interface ICallableFilter
    {
        Task<bool> InvokeAsync(IMiniAppContext context, CancellationToken cancellationToken);
    }
}
