using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpCookies : Dictionary<string, HttpCookie>
    {
        public HttpCookies()
        {
        }

        public HttpCookies(IDictionary<string, HttpCookie> cookies): base(cookies) 
        {
        }

        public HttpCookies(IEnumerable<HttpCookie> cookies)
        {
            ArgumentNullException.ThrowIfNull(cookies);

            foreach (var cookie in cookies)
            {
                if (!ContainsKey(cookie.Name))
                    Add(cookie.Name, cookie);
                else
                    this[cookie.Name] = cookie;
            }
        }
    }
}
