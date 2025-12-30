using MiniWebServer.Abstractions;

namespace MiniWebServer.OutputCaching;

public class OutputCachePolicyBuilder : IOutputCachePolicyBuilder
{
    private string? name;
    private readonly List<HttpResponseCodes> httpResponseCodes = [];
    private readonly List<Abstractions.Http.HttpMethod> methods = [];
    private TimeSpan? expire;
    private Func<string, bool>? pathMatching;

    public IOutputCachePolicyBuilder SetName(string name)
    {
        this.name = name ?? throw new ArgumentNullException(nameof(name));

        return this;
    }
    public IOutputCachePolicyBuilder AddHttpResponseCode(HttpResponseCodes responseCode)
    {
        httpResponseCodes.Add(responseCode);

        return this;
    }

    public IOutputCachePolicyBuilder AddHttpResponseCode(IEnumerable<HttpResponseCodes> responseCodes)
    {
        httpResponseCodes.AddRange(responseCodes);

        return this;
    }

    public IOutputCachePolicyBuilder AddMethod(Abstractions.Http.HttpMethod method)
    {
        methods.Add(method);

        return this;
    }

    public IOutputCachePolicyBuilder AddMethod(IEnumerable<Abstractions.Http.HttpMethod> methods)
    {
        this.methods.AddRange(methods);

        return this;
    }

    public IOutputCachePolicy Build()
    {
        var policy = new OutputCachePolicy(
            pathMatching ?? (path => false),
            methods.Count != 0 ? methods : null, // null to use default values
            httpResponseCodes.Count != 0 ? httpResponseCodes : null,
            expire
            );

        return policy;
    }

    public IOutputCachePolicyBuilder SetExpire(TimeSpan expire)
    {
        this.expire = expire;

        return this;
    }

    public IOutputCachePolicyBuilder SetPathMatching(Func<string, bool> pathMatching)
    {
        this.pathMatching = pathMatching;

        return this;
    }

}
