using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction.ActionResults
{
    public class ViewActionResult : IActionResult
    {
        private readonly string viewName;
        private readonly string contentType;
        private readonly object? model;
        private readonly IDictionary<string, object> viewData;
        private readonly IViewEngine viewEngine;

        public ViewActionResult(string viewName, object? model, string contentType, IDictionary<string, object> viewData, IViewEngine viewEngine)
        {
            this.viewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
            this.model = model;
            this.contentType = contentType ?? "text/html";
            this.viewData = viewData ?? new Dictionary<string, object>();
            this.viewEngine = viewEngine ?? throw new ArgumentNullException(nameof (viewEngine));
        }

        public Task ExecuteResultAsync(IMiniAppContext context)
        {
            return Task.CompletedTask;
        }
    }
}
