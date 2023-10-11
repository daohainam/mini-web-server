using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction
{
    public class ActionInfo
    {
        public ActionInfo(string actionName, MethodInfo methodInfo, Type controllerType)
        {
            ActionName = actionName ?? throw new ArgumentNullException(nameof(actionName));
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            ControllerType = controllerType ?? throw new ArgumentNullException(nameof(controllerType));
        }

        public string ActionName { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public Type ControllerType { get; set; }
    }
}
