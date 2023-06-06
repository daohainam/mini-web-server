using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Web.StaticFileSupport
{
    public class NotFoundCallable : BaseCallable
    {
        public static NotFoundCallable Instance => new(); // BaseCallable always returns 404 Not Found
    }
}
