using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class RedirectActionResult : IActionResult
    {
        private readonly string url;
        private readonly bool permanent;

        public RedirectActionResult(string url, bool permanent) { 
            this.url = url;
            this.permanent = permanent;
        }

        public Task ExecuteResultAsync(ActionResultContext context)
        {
            context.Response.StatusCode = permanent ? Abstractions.HttpResponseCodes.PermanentRedirect : Abstractions.HttpResponseCodes.TemporaryRedirect;
            context.Response.Headers.Location = url;
            context.Response.Content = MiniApp.Content.StringContent.Empty;

            return Task.CompletedTask;
        }
    }
}
