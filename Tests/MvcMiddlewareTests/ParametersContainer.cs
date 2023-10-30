using MiniWebServer.Abstractions;
using MiniWebServer.Abstractions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcMiddlewareTests
{
    internal class ParametersContainer : IParametersContainer
    {
        public required HttpParameters QueryParameters { get; init; }
    }
}
