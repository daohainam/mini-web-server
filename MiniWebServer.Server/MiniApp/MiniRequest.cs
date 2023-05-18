using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniRequest : IMiniAppRequest
    {
        public MiniRequest(IAppContext context, HttpRequest httpRequest)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            this.httpRequest = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
        }

        public IAppContext Context { get; }

        private readonly HttpRequest httpRequest;

        public Cookie[] Cookies => throw new NotImplementedException();

        public string Url => httpRequest.Url;
    }
}
