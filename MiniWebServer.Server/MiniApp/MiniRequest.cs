using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.MiniApp;
using MiniWebServer.Server.BodyReaders.Form;
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
        public MiniAppConnectionContext ConnectionContext { get; }
        private readonly IHttpRequest httpRequest;


        public MiniRequest(MiniAppConnectionContext connectionContext, IHttpRequest httpRequest)
        {
            this.ConnectionContext = connectionContext ?? throw new ArgumentNullException(nameof(connectionContext));
            this.httpRequest = httpRequest ?? throw new ArgumentNullException(nameof(httpRequest));
            
            BodyManager = new MiniBodyManager(httpRequest.BodyPipeline.Reader);
        }

        public string Url => httpRequest.Url;
        public string QueryString => httpRequest.QueryString;
        public HttpParameters QueryParameters => httpRequest.QueryParameters;
        public HttpCookies Cookies => httpRequest.Cookies;
        public string Hash => httpRequest.Hash;
        public HttpHeaders Headers => httpRequest.Headers;
        public HttpMethod Method => httpRequest.Method;
        public IMiniBodyManager BodyManager { get; }
        public long ContentLength => httpRequest.ContentLength;
        public string ContentType => httpRequest.ContentType;
        public async Task<IRequestForm> ReadFormAsync(CancellationToken cancellationToken = default)
        {
            var form = new RequestForm();
            
            if (httpRequest.ContentType == null)
                return form;

            var reader = BodyManager.GetReader();

            if (reader == null)
            {
                // empty form
                return form;
            }

            var formReader = ConnectionContext.FormReaderFactory.CreateFormReader(httpRequest.ContentType, httpRequest.ContentLength, ConnectionContext.LoggerFactory) ?? throw new InvalidHttpStreamException("Not supported content type");
            var readform = await formReader.ReadAsync(reader, cancellationToken);
            return readform ?? throw new InvalidHttpStreamException("Error reading form data");
        }
    }
}
