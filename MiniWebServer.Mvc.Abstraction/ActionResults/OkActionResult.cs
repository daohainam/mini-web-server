using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class OkActionResult : IActionResult
    {
        private readonly object? value;
        public OkActionResult(object? value) { 
            this.value = value;
        }

        public Task ExecuteResultAsync(ActionResultContext context)
        {
            context.Response.StatusCode = Abstractions.HttpResponseCodes.OK; 

            var valueString = value?.ToString() ?? string.Empty;
            context.Response.Content = new MiniApp.Content.StringContent(valueString);

            return Task.CompletedTask;
        }
    }
}
