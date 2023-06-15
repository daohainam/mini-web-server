using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions.Http;
using MiniWebServer.Abstractions.Http.Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    public static class FormReaderExtensions
    {
        public static async Task<IRequestForm> ReadFormAsync(this IHttpRequest request, ILoggerFactory? loggerFactory = default, CancellationToken cancellationToken = default)
        {
            var form = new RequestForm();

            if (request.ContentType == null)
                return form;

            var reader = request.BodyManager.GetReader();

            if (reader == null)
            {
                // empty form
                return form;
            }

            var formReaderFactory = new DefaultFormReaderFactory(loggerFactory);

            var formReader = formReaderFactory.CreateFormReader(request.ContentType, request.ContentLength) ?? throw new InvalidHttpStreamException("Not supported content type");
            var readform = await formReader.ReadAsync(reader, cancellationToken);
            return readform ?? throw new InvalidHttpStreamException("Error reading form data");
        }
    }
}
