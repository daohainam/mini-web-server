using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcMiddlewareTests
{
    internal class FormContainer : IFormContainer
    {
        private readonly IRequestForm form;

        public FormContainer(IRequestForm? form = default)
        {
            this.form = form ?? new RequestForm();
        }

        public Task<IRequestForm> ReadFormAsync(ILoggerFactory? loggerFactory = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(form);
        }
    }
}
