using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    public interface IFormReaderFactory
    {
        IFormReader? CreateFormReader(string contentType, long contentLength);
    }
}
