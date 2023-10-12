using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction.ViewContent
{
    public class StringViewContent : IViewContent
    {
        private readonly string content;

        public StringViewContent(string content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public Task RenderAsync(ActionResultContext context)
        {
            context.Response.Content = new MiniApp.Content.StringContent(content ?? string.Empty);
            context.Response.StatusCode = Abstractions.HttpResponseCodes.OK;

            return Task.CompletedTask;
        }
    }
}
