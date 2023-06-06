using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp
{
    public abstract class BaseCallable : ICallable
    {
        public virtual Task Connect(IMiniAppContext context, CancellationToken cancellationToken)
        {
            context.Response.SetStatus(Abstractions.HttpResponseCodes.NotFound);

            return Task.CompletedTask;
        }

        public virtual Task Delete(IMiniAppContext context, CancellationToken cancellationToken)
        {
            context.Response.SetStatus(Abstractions.HttpResponseCodes.NotFound);

            return Task.CompletedTask;
        }

        public virtual Task Get(IMiniAppContext context, CancellationToken cancellationToken)
        {
            context.Response.SetStatus(Abstractions.HttpResponseCodes.NotFound);

            return Task.CompletedTask;
        }

        public virtual Task Head(IMiniAppContext context, CancellationToken cancellationToken)
        {
            context.Response.SetStatus(Abstractions.HttpResponseCodes.NotFound);

            return Task.CompletedTask;
        }

        public Task Options(IMiniAppContext context, CancellationToken cancellationToken)
        {
            context.Response.SetStatus(Abstractions.HttpResponseCodes.NotFound);

            return Task.CompletedTask;
        }

        public virtual Task Post(IMiniAppContext context, CancellationToken cancellationToken)
        {
            context.Response.SetStatus(Abstractions.HttpResponseCodes.NotFound);

            return Task.CompletedTask;
        }

        public virtual Task Put(IMiniAppContext context, CancellationToken cancellationToken)
        {
            context.Response.SetStatus(Abstractions.HttpResponseCodes.NotFound);

            return Task.CompletedTask;
        }

        public virtual Task Trace(IMiniAppContext context, CancellationToken cancellationToken)
        {
            context.Response.SetStatus(Abstractions.HttpResponseCodes.NotFound);

            return Task.CompletedTask;
        }
    }
}
