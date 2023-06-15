using HttpMultipartParser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http.Form
{
    public class MultipartFormDataFormReader : IFormReader
    {
        private readonly string boundary;
        private readonly ILogger<MultipartFormDataFormReader> logger;
        private RequestForm? form;

        public MultipartFormDataFormReader(string boundary, ILoggerFactory? loggerFactory)
        {
            this.boundary = boundary;

            if (loggerFactory != null)
            {
                logger = loggerFactory.CreateLogger<MultipartFormDataFormReader>();
            }
            else
            {
                logger = NullLogger<MultipartFormDataFormReader>.Instance;
            }
        }

        public async Task<IRequestForm?> ReadAsync(PipeReader pipeReader, CancellationToken cancellationToken = default)
        {
            form = new RequestForm();
            var parser = new StreamingMultipartFormDataParser(
                pipeReader.AsStream(),
                boundary: boundary
                );

            parser.ParameterHandler += this.OnParameterFound;
            parser.FileHandler += this.OnFilePartFound;

            await parser.RunAsync(cancellationToken);

            return form;
        }

        private void OnFilePartFound(string name, string fileName, string contentType, string contentDisposition, byte[] buffer, int bytes, int partNumber, IDictionary<string, string> additionalProperties)
        {
            if (form == null)
            {
                throw new NullReferenceException("form must be not null");
            }

            logger.LogDebug("Found file: {p}, fileName: {fn}", name, fileName);
        }

        private void OnParameterFound(ParameterPart part)
        {
            if (form == null)
            {
                throw new NullReferenceException("form must be not null");
            }

            logger.LogDebug("Found parameter: {p}={v}", part.Name, part.Data);

            form[part.Name] = part.Data;
        }
    }
}
