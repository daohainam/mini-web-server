using MiniWebServer.Abstractions;

namespace MiniWebServer.OutputCaching
{
    public interface IOutputCachePolicy
    {
        IEnumerable<Abstractions.Http.HttpMethod> Methods { get; set; }
        Func<string, bool> PathMatching { get; set; }
        IEnumerable<HttpResponseCodes> HttpResponseCodes { get; set; }
        TimeSpan? Expire { get; set; }
    }
}