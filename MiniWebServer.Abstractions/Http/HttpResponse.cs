﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpResponse : IHttpResponse
    {
        private HttpResponseCodes statusCode;

        public HttpResponse()
        {
            StatusCode = HttpResponseCodes.NotFound;
            ReasonPhrase = HttpResponseReasonPhrases.GetReasonPhrase(HttpResponseCodes.NotFound);
            Headers = new();
            Cookies = new();
            Content = EmptyContent.Instance;
        }
        public HttpResponse(HttpResponseCodes statusCode, string? reasonPhrase = null, HttpResponseHeaders? headers = null, HttpCookies? cookies = null, IHttpContent? content = null)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase ?? HttpResponseReasonPhrases.GetReasonPhrase(statusCode);
            Headers = headers ?? new HttpResponseHeaders();
            Cookies = cookies ?? new HttpCookies();
            Content = content ?? EmptyContent.Instance;
        }

        public HttpResponseCodes StatusCode { 
            get {
                return statusCode;
            } 
            set { 
                statusCode = value;
                ReasonPhrase = HttpResponseReasonPhrases.GetReasonPhrase(value);
            } 
        }
        public string ReasonPhrase { get; set; }
        public HttpResponseHeaders Headers { get; }
        public IHttpContent Content { get; set; }
        public HttpCookies Cookies { get; }
    }
}
