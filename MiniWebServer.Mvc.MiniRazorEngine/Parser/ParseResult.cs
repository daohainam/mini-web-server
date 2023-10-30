using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.MiniRazorEngine.Parser
{
    public class ParseResult
    {
        public ParseResult(bool compiled, string compiledContent, Exception? exception)
        {
            Compiled = compiled;
            CompiledContent = compiledContent ?? throw new ArgumentNullException(nameof(compiledContent));
            Exception = exception;  
        }

        public ParseResult(bool compiled, string compiledContent) : this(compiled, compiledContent, null) { }
        public ParseResult(bool compiled, Exception exception) : this(compiled, string.Empty, exception) { }

        public bool Compiled { get; }
        public string CompiledContent { get; }
        public Exception? Exception { get; }
    }
}
