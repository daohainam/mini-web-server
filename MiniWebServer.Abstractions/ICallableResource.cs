using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    /// <summary>
    /// a ICallableResource is a resource which you can call and make a response based on that, we don't care what it is, it is just an abstract thing
    /// An implementation can be a PHP engine, a javascript engine or anything else...
    /// We don't have to apply abstraction to a deeper level since HTTP is a standard and we all know there will be no other methods except what we have 
    /// defined (in the standard)
    /// </summary>
    public interface ICallableResource
    {
        Task OnGet(HttpRequest request, IHttpResponseBuilder responseObjectBuilder, IMimeTypeMapping mimeTypeMappings);
    }
}
