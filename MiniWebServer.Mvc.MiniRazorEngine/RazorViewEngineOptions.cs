namespace MiniWebServer.Mvc.RazorEngine;

public class MiniRazorViewEngineOptions
{
    public string TempDirectory { get; set; } = ".tmp";
    public string AssembyCacheDirectory => Path.Combine(TempDirectory, "asmcache");
}
