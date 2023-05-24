using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniRequest : IMiniAppRequest
    {
        private readonly IHttpRequest httpRequest;

        public MiniRequest(IAppContext context, 
            IHttpRequest httpRequest)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            this.httpRequest = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
        }

        public IAppContext Context { get; }

        public string Url => httpRequest.Url;
        public string QueryString => httpRequest.QueryString;
        public HttpParameters QueryParameters => httpRequest.QueryParameters;
        public HttpCookies Cookies => httpRequest.Cookies;
        public string Hash => httpRequest.Hash;
        public HttpHeaders Headers => httpRequest.Headers;
        public HttpMethod Method => httpRequest.Method;
        public PipeReader? BodyReader => httpRequest.BodyReader;
    }
}
