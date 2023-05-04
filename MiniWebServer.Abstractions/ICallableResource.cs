using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    public interface ICallableResource
    {
        Task OnGet(HttpRequest request, IHttpResponseBuilder responseObjectBuilder);
    }
}
