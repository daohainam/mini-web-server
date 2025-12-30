using System.Reflection;

namespace MiniWebServer.Mvc.Abstraction;

public class ActionInfo(string actionName, MethodInfo methodInfo, Type controllerType)
{
    public string ActionName { get; set; } = actionName ?? throw new ArgumentNullException(nameof(actionName));
    public MethodInfo MethodInfo { get; set; } = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
    public Type ControllerType { get; set; } = controllerType ?? throw new ArgumentNullException(nameof(controllerType));
}
