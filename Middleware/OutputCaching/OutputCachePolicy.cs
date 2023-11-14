using MiniWebServer.Abstractions;
using HttpMethod = MiniWebServer.Abstractions.Http.HttpMethod;

namespace MiniWebServer.OutputCaching
{
    public class OutputCachePolicy : IOutputCachePolicy
    {
        public OutputCachePolicy(Func<string, bool> pathMatching, IEnumerable<HttpMethod>? methods, IEnumerable<HttpResponseCodes>? httpResponseCodes = null, TimeSpan? expire = null)
        {
            PathMatching = pathMatching ?? throw new ArgumentNullException(nameof(pathMatching));

            Methods = methods ?? DefaultHttpMethods;
            HttpResponseCodes = httpResponseCodes ?? DefaultHttpResponseCodes;
            Expire = expire;
        }

        public IEnumerable<HttpMethod> Methods { get; set; }
        public Func<string, bool> PathMatching { get; set; }
        public IEnumerable<HttpResponseCodes> HttpResponseCodes { get; set; }
        public TimeSpan? Expire { get; set; }

        private static readonly HttpResponseCodes[] DefaultHttpResponseCodes = { Abstractions.HttpResponseCodes.OK };
        private static readonly HttpMethod[] DefaultHttpMethods = { HttpMethod.Head, HttpMethod.Get };
    }
}
