using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction.ViewContent
{
    public class InternalErrorViewContent : IViewContent
    {
        private readonly string content;

        public InternalErrorViewContent(string content = "")
        {
            this.content = content ?? string.Empty;
        }

        public Task RenderAsync(ActionResultContext context)
        {
            context.Response.Content = new MiniApp.Content.StringContent(content);
            context.Response.StatusCode = Abstractions.HttpResponseCodes.InternalServerError;

            return Task.CompletedTask;
        }
    }
}
