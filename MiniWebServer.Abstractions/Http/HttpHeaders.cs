using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpHeaders : IEnumerable<KeyValuePair<string, HttpHeader>>
    {
        // we have a list of header, but we use a dictionary because we will do a lot of search on this.
        // using a List might save some memories, but slower
        private readonly IDictionary<string, HttpHeader> headers = new Dictionary<string, HttpHeader>(); 

        public HttpHeaders Add(string name, string value) // we return a HttpHeaders so we can make a chain of call, for example: headers.Add("Connection", "Close").Add("ETag", "ABCD") ...
        {
            headers[name] = new HttpHeader(name, value);

            return this;
        }

        public HttpHeaders Add(string name, IEnumerable<string> value)
        {
            headers[name] = new HttpHeader(name, value);

            return this;
        }

        public HttpHeaders Add(string name, HttpHeader value)
        {
            headers[name] = value ?? throw new ArgumentNullException(nameof(value));

            return this;
        }

        public bool TryGetValue(string name, out HttpHeader? header)
        {
            return headers.TryGetValue(name, out header);
        }

        public IEnumerator<KeyValuePair<string, HttpHeader>> GetEnumerator()
        {
            return headers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
