using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Web.Mvc
{
    // I made this as a framework for my ideas, nothing implemented yet
    public class MvcCallableService : ICallableService
    {
        public bool IsGetSupported => false;

        public ICallable? Find(IMiniAppRequest request)
        {
            return null;
        }
    }
}
