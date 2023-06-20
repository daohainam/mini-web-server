using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MiniWebServer.MiniApp.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.ResponseCompression
{
    public static class ResponseCompressionMiddlewareExtensions
    {
        public static void UseResponseCompression(this IMiniAppBuilder appBuilder, ResponseCompressionOptions? options = default)
        {
            options ??= new ResponseCompressionOptions();

            appBuilder.Services.AddTransient(services => new ResponseCompressionMiddleware(
                options
                ));

            appBuilder.UseMiddleware<ResponseCompressionMiddleware>();
        }
    }
}
