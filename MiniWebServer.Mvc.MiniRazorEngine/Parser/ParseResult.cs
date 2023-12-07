namespace MiniWebServer.Mvc.MiniRazorEngine.Parser
{
    public class ParseResult(bool compiled, string compiledContent, Exception? exception)
    {
        public ParseResult(bool compiled, string compiledContent) : this(compiled, compiledContent, null) { }
        public ParseResult(bool compiled, Exception exception) : this(compiled, string.Empty, exception) { }

        public bool Compiled { get; } = compiled;
        public string CompiledContent { get; } = compiledContent ?? throw new ArgumentNullException(nameof(compiledContent));
        public Exception? Exception { get; } = exception;
    }
}
