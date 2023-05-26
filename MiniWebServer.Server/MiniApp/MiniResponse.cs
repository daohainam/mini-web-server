using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.Server.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.MiniApp
{
    public class MiniResponse : IMiniAppResponse
    {
        public MiniResponse(IAppContext context, IHttpResponseBuilder responseBuilder)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            this.responseBuilder = responseBuilder ?? throw new ArgumentNullException(nameof(responseBuilder));
        }

        public IAppContext Context { get; }
        private readonly IHttpResponseBuilder responseBuilder;

        public void AddHeader(string name, string mimeType)
        {
            responseBuilder.AddHeader(name, mimeType);
        }

        public void SetStatus(HttpResponseCodes statusCode, string reasonPhrase)
        {
            responseBuilder.SetStatusCode(statusCode);
            responseBuilder.SetReasonPhrase(reasonPhrase);
        }

        public void SetStatus(HttpResponseCodes statusCode)
        {
            responseBuilder.SetStatusCode(statusCode);
            responseBuilder.SetReasonPhrase(HttpResponseReasonPhrases.ReasonPhrases.GetValueOrDefault(statusCode) ?? string.Empty);
        }

        public void SetContent(MiniContent content)
        {
            responseBuilder.SetContent(content);
            responseBuilder.SetHeaderContentLength(content.ContentLength);
        }
    }
}
