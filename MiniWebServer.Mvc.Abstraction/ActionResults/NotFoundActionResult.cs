using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class NotFoundActionResult : IActionResult
    {
        private readonly object? value;
        public NotFoundActionResult(object? value) { 
            this.value = value;
        }

        public Task ExecuteResultAsync(ActionResultContext context)
        {
            context.Response.StatusCode = Abstractions.HttpResponseCodes.NotFound; 

            if (value != null)
            {
                var valueString = value.ToString();
                context.Response.Content = MiniApp.Content.StringContent.FromValue(valueString);
            }
            else
            {
                context.Response.Content = MiniApp.Content.StringContent.Empty;
            }

            return Task.CompletedTask;
        }
    }
}
