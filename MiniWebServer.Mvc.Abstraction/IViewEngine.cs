using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction
{
    public interface IViewEngine
    {
        // generate content data from a model using viewName to find templates
        Task<IViewContent?> RenderAsync(string viewName, object? model, IDictionary<string, object> viewData);
    }
}
