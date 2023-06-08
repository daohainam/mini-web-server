using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MiniWebServer.Session;

namespace Middleware.Session.Tests
{
    [TestClass]
    public class DistributedCacheSessionTests
    {
        [TestMethod]
        public void StringTests()
        {
            Dictionary<string, string> keyValuePairs = new();
            keyValuePairs["key1"] = "value1";
            keyValuePairs["key2"] = "v32r2t43qyqyb qn43n3n36nnqn6q4";
            keyValuePairs["key3"] = "bq253q53wb4n   yw4 7y5w4 7yw4 yw yw ";
            keyValuePairs["key4"] = "\r\n\t\0";
            keyValuePairs["key5"] = "!#¤%&/()=?";

            var services = new ServiceCollection();

            services.AddDistributedMemoryCache();
            var sp = services.BuildServiceProvider();

            var store = new DistributedCacheSessionStore(sp.GetRequiredService<IDistributedCache>());
            var session = store.Create("111-222-333-444");

            foreach (var kv in keyValuePairs)
            {
                session.SetString(kv.Key, kv.Value);
            }

            foreach (var kv in keyValuePairs)
            {
                var value = session.GetString(kv.Key);

                Assert.AreEqual(kv.Value, value);
            }
        }
    }
}