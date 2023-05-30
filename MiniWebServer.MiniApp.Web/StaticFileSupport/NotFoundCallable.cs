using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Web.StaticFileSupport
{
    public class NotFoundCallable : ICallable
    {
        public async Task Get(IMiniAppContext context, CancellationToken cancellationToken)
        {
            await Task.Run(() => {
                context.Response.SetStatus(HttpResponseCodes.NotFound);
            }, cancellationToken);
        }

        public async Task Post(IMiniAppContext context, CancellationToken cancellationToken)
        {
            await Task.Run(() => {
                context.Response.SetStatus(HttpResponseCodes.NotFound);
            }, cancellationToken);
        }

        public static NotFoundCallable Instance => new();
    }
}
