using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using MiniWebServer.Mvc.Abstraction.ActionResults;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction
{
    public abstract class Controller: IController
    {
        private ControllerContext? controllerContext = null;
        public ControllerContext ControllerContext { 
            get
            {
                if (controllerContext == null)
                {
                    throw new InvalidOperationException("ControllerContext must be inited before executing actions");
                }

                return controllerContext;
            }
            set
            {
                ArgumentNullException.ThrowIfNull(nameof(value));

                controllerContext = value;
            } 
        }

        public IMiniAppContext Context => ControllerContext.Context;
        public IHttpRequest Request => ControllerContext.Context.Request;
        public IHttpResponse Response => ControllerContext.Context.Response;
        public ISession Session => ControllerContext.Context.Session;
        public ClaimsPrincipal? User => ControllerContext.Context.User;

        protected virtual IActionResult BadRequest(object? content)
        {
            return new BadRequestActionResult(content);
        }
        protected virtual IActionResult Json(object content)
        {
            return new JsonActionResult(content, null);
        }
        protected virtual IActionResult Json(object content, JsonSerializerOptions jsonSerializerOptions)
        {
            return new JsonActionResult(content, jsonSerializerOptions);
        }
        protected virtual IActionResult NotFound(object? content)
        {
            return new NotFoundActionResult(content);
        }
        protected virtual IActionResult Ok(object? content)
        {
            return new OkActionResult(content);
        }
        protected virtual IActionResult Redirect(string url)
        {
            return new RedirectActionResult(url, false);
        }
        protected virtual IActionResult RedirectPermanent(string url)
        {
            return new RedirectActionResult(url, true);
        }
        protected virtual IActionResult View(string viewName, object? model, IDictionary<string, object>? viewData, string? contentType = default)
        {
            return new ViewActionResult(controllerContext!, viewName, model, contentType ?? "text/html", viewData ?? new Dictionary<string, object>(), controllerContext!.ViewEngine);
        }
        protected virtual IActionResult View() // we don't use CallerMemberName since it interferes with method parameter (object model) 
        {
            var callerName = (new StackFrame(1)?.GetMethod()?.Name) ?? throw new InvalidOperationException("Cannot get method name");
            return View(callerName, null, null, null);
        }
        protected virtual IActionResult View(object? model)
        {
            var callerName = (new StackFrame(1)?.GetMethod()?.Name) ?? throw new InvalidOperationException("Cannot get method name");

            return View(callerName, model, null, null);
        }
        protected virtual IActionResult View(string viewName, object model)
        {
            return View(viewName, model, null, null);
        }
    }
}
