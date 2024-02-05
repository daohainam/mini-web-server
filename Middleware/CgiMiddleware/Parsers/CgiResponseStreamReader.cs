using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi.Parsers
{
    internal class CgiResponseStreamReader : ICgiResponseReader
    {
        private readonly StreamReader reader;
        private readonly ILogger logger;

        public CgiResponseStreamReader(StreamReader reader, ILogger logger) { 
            this.reader = reader;
            this.logger = logger;
        }
        public async Task<CgiResponse?> ReadAsync()
        {
            string cgiOutput = await reader.ReadToEndAsync();

            return new CgiResponse() { 
                ResponseCode = Abstractions.HttpResponseCodes.OK,
                Content = new MiniApp.Content.StringContent(cgiOutput),
                Headers = new()
            };
        }
    }
}
