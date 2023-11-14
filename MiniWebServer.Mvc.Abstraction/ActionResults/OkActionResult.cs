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

            if (value != null)
            {
                if (value.GetType() == typeof(string))
                {
                    context.Response.Content = MiniApp.Content.StringContent.FromValue((string)value);
                }
                else
                {
                    var valueString = value.ToString();
                    context.Response.Content = MiniApp.Content.StringContent.FromValue(valueString);
                }
            }
            else
            {
                context.Response.Content = MiniApp.Content.StringContent.Empty;
            }

            return Task.CompletedTask;
        }
    }
}
