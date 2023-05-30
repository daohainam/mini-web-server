using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.BodyReaders.Form
{
    public class DefaultFormReaderFactory : IFormReaderFactory
    {
        private const string MultipartFormDataContentType = "multipart/form-data";
        private const string XWwwFormUrlEncodedContentType = "application/x-www-form-urlencoded";

        public IFormReader? CreateFormReader(string contentType, ILoggerFactory loggerFactory)
        {
            ArgumentException.ThrowIfNullOrEmpty(contentType);

            var contentTypeParts = contentType.Split(';');

            if (contentTypeParts.Length ==  0) 
                return null;

            if (contentTypeParts[0] == MultipartFormDataContentType) 
            {
                // we need at least 2 parts, for example: Content-Type: multipart/form-data; boundary=--------------------------828808808945687226760206
                if (contentTypeParts.Length < 2)
                    return null;

                string boundary = contentTypeParts[1];
                if (!boundary.StartsWith(" boundary="))
                    return null;

                return new MultipartFormDataFormReader(boundary[10..], loggerFactory);
            }
            else if (contentTypeParts[0] == XWwwFormUrlEncodedContentType)
            {
                return new XWwwFormUrlencodedFormReader();
            }
            else
            {
                return null;
            }
        }
    }
}
