using Microsoft.Extensions.Logging;

namespace MiniWebServer.Abstractions.Http.Form
{
    public class DefaultFormReaderFactory(ILoggerFactory? loggerFactory) : IFormReaderFactory
    {
        private const string MultipartFormDataContentType = "multipart/form-data";
        private const string XWwwFormUrlEncodedContentType = "application/x-www-form-urlencoded";

        public IFormReader? CreateFormReader(string contentType, long contentLength)
        {
            ArgumentException.ThrowIfNullOrEmpty(contentType);

            var contentTypeParts = contentType.Split(';');

            if (contentTypeParts.Length == 0)
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
                return new XWwwFormUrlencodedFormReader(contentLength);
            }
            else
            {
                return null;
            }
        }
    }
}
