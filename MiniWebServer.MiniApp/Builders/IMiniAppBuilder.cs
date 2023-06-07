using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Builders
{
    public interface IMiniAppBuilder
    {
        IMiniApp Build();
        IServiceCollection Services { get; }
        IMiniAppBuilder UseMiddleware(Type middlewareType);
        IMiniAppBuilder UseMiddleware<TMiddleware>();
    }
}
