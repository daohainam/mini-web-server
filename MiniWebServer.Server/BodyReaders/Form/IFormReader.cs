using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.BodyReaders.Form
{
    public interface IFormReader
    {
        Task<IRequestForm?> ReadAsync(PipeReader pipeReader, CancellationToken cancellationToken = default);
    }
}
