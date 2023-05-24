using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpCookies : IReadOnlyDictionary<string, HttpCookie>
    {
        private readonly Dictionary<string, HttpCookie> cookies;

        public HttpCookies()
        {
            this.cookies = new Dictionary<string, HttpCookie>();
        }

        public HttpCookies(Dictionary<string, HttpCookie> cookies)
        {
            this.cookies = cookies ?? throw new ArgumentNullException(nameof(cookies));
        }

        public HttpCookies(IEnumerable<HttpCookie> cookies)
        {
            if (cookies == null) 
                throw new ArgumentNullException(nameof(cookies));

            var d = new Dictionary<string, HttpCookie>();
            foreach (var cookie in cookies)
            {
                if (!d.ContainsKey(cookie.Name))
                    d.Add(cookie.Name, cookie);
                else
                    d[cookie.Name] = cookie;
            }

            this.cookies = d;
        }

        public HttpCookie this[string key] => cookies[key];

        public IEnumerable<string> Keys => cookies.Keys;

        public IEnumerable<HttpCookie> Values => cookies.Values;

        public int Count => cookies.Count;

        public bool ContainsKey(string key)
        {
            return cookies.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, HttpCookie>> GetEnumerator()
        {
            return cookies.GetEnumerator();
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out HttpCookie value)
        {
            return cookies.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return cookies.GetEnumerator();
        }
    }
}
