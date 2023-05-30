using HttpMultipartParser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MiniWebServer.MiniApp;
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

namespace MiniWebServer.Server.BodyReaders.Form
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
            //ReadResult readResult = await pipeReader.ReadAsync(cancellationToken);
            //ReadOnlySequence<byte> buffer = readResult.Buffer;
            //var ms = new FileStream("debug-stream.txt", FileMode.Create, FileAccess.Write);
            //long total = 0;

            //while (true)
            //{
            //    total += buffer.Length;
            //    ms.Write(buffer.ToArray());

            //    pipeReader.AdvanceTo(buffer.End);

            //    logger.LogDebug("Write {length} bytes, total {total}", buffer.Length, total);

            //    if (readResult.IsCompleted)
            //        break;

            //    readResult = await pipeReader.ReadAsync(cancellationToken);
            //    buffer = readResult.Buffer;
            //}
            //ms.Flush();
            //ms.Close();


            //var stream = pipeReader.AsStream();
            //var ms = new FileStream("debug-stream.txt", FileMode.Create, FileAccess.Write);
            //var buffer = new byte[4096];

            //var l = stream.Read(buffer);
            //while (l > 0)
            //{
            //    ms.Write(buffer, 0, l);
            //    l = stream.Read(buffer);
            //}

            //ms.Flush();
            //ms.Close();

            //ms = new FileStream("debug-stream.txt", FileMode.Open, FileAccess.Read);

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
