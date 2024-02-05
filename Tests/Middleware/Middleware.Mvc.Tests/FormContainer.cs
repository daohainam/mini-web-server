using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;

namespace MvcMiddlewareTests
{
    internal class FormContainer(IRequestForm? form = default) : IFormContainer
    {
        private readonly IRequestForm form = form ?? new RequestForm();

        public Task<IRequestForm> ReadFormAsync(ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(form);
        }
    }
}
