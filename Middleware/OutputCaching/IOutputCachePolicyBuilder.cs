using MiniWebServer.Abstractions;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.OutputCaching;

public interface IOutputCachePolicyBuilder
{
    IOutputCachePolicyBuilder SetName(string name);
    IOutputCachePolicyBuilder AddMethod(HttpMethod method);
    IOutputCachePolicyBuilder AddMethod(IEnumerable<HttpMethod> methods);
    IOutputCachePolicyBuilder SetPathMatching(Func<string, bool> pathMatching);
    IOutputCachePolicyBuilder AddHttpResponseCode(HttpResponseCodes responseCode);
    IOutputCachePolicyBuilder AddHttpResponseCode(IEnumerable<HttpResponseCodes> responseCodes);
    IOutputCachePolicyBuilder SetExpire(TimeSpan expire);

    IOutputCachePolicy Build();
}
