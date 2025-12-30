using MiniWebServer.Abstractions;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.OutputCaching;

public class OutputCachePolicy(Func<string, bool> pathMatching, IEnumerable<HttpMethod>? methods, IEnumerable<HttpResponseCodes>? httpResponseCodes = null, TimeSpan? expire = null) : IOutputCachePolicy
{
    public IEnumerable<HttpMethod> Methods { get; set; } = methods ?? DefaultHttpMethods;
    public Func<string, bool> PathMatching { get; set; } = pathMatching ?? throw new ArgumentNullException(nameof(pathMatching));
    public IEnumerable<HttpResponseCodes> HttpResponseCodes { get; set; } = httpResponseCodes ?? DefaultHttpResponseCodes;
    public TimeSpan? Expire { get; set; } = expire;

    private static readonly HttpResponseCodes[] DefaultHttpResponseCodes = [Abstractions.HttpResponseCodes.OK];
    private static readonly HttpMethod[] DefaultHttpMethods = [HttpMethod.Head, HttpMethod.Get];
}
