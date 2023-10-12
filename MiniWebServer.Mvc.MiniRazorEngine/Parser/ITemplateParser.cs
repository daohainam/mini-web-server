using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.MiniRazorEngine.Parser
{
    // parse a string (view file content) to a C# source code
    public interface ITemplateParser
    {
        bool TryParse(string template, out string? sourceCode);
    }
}
