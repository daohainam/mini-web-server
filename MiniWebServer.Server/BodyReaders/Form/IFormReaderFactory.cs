using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.BodyReaders.Form
{
    public interface IFormReaderFactory
    {
        IFormReader? CreateFormReader(string contentType, ILoggerFactory loggerFactory);
    }
}
