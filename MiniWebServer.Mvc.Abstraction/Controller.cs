using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction
{
    public abstract class Controller: IController
    {
        private ControllerContext? controllerContext = null;
        public ControllerContext ControllerContext { 
            get
            {
                if (controllerContext == null)
                {
                    throw new InvalidOperationException("ControllerContext must be inited before executing actions");
                }

                return controllerContext;
            }
            set
            {
                ArgumentNullException.ThrowIfNull(nameof(value));

                controllerContext = value;
            } 
        }

        public IMiniAppContext Context => ControllerContext.Context;
        public IHttpRequest Request => ControllerContext.Context.Request;
        public IHttpResponse Response => ControllerContext.Context.Response;
        public ISession Session => ControllerContext.Context.Session;
        public ClaimsPrincipal? User => ControllerContext.Context.User;

    }
}
