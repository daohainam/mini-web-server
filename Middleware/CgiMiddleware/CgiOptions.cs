using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Cgi;

public class CgiOptions
{
    public CgiHandler[] Handlers { get; set; } = [];
}

public class CgiHandler
{
    public string Route { get; set; } = string.Empty;
    public string Executable { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public string ScriptDirectory { get; set; } = string.Empty;
    public string[] Parameters { get; set; } = []; // additional paramaters sent to executable, first parameter is always script file

    // TODO: we should support context switching, running an executable file in the same server context is potentially unsafe
    public CgiHandlerSecurityContext? SecurityContext { get; set; }
}

public class CgiHandlerSecurityContext
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // we need to refactor to SecureString later
    public string Domain { get; set; } = string.Empty;
    public bool LoadUserProfile { get; set; }
}
