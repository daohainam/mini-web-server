using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpHeaders : IEnumerable<KeyValuePair<string, HttpHeader>>
    {
        // we have a list of header, but we use a dictionary because we will do a lot of search on this.
        // using a List might save some memories, but slower
        private readonly IList<HttpHeader> headers = new List<HttpHeader>(); 

        public HttpHeaders Add(string name, string value) // we return a HttpHeaders so we can make a chain of call, for example: headers.Add("Connection", "Close").Add("ETag", "ABCD") ...
        {
            headers.Add(new HttpHeader(name, value));

            return this;
        }

        public HttpHeaders Add(string name, IEnumerable<string> value)
        {
            headers.Add(new HttpHeader(name, value));

            return this;
        }

        public HttpHeaders Add(HttpHeader value)
        {
            headers.Add(value);

            return this;
        }

        public bool TryGetValue(string name, out HttpHeader? header)
        {
            var h = headers.Where(h => h.Name == name).FirstOrDefault();
            if (h != null)
            {
                header = h;
                return true;
            }
            else {
                header = null;
                return false; 
            }
        }

        public IEnumerator<KeyValuePair<string, HttpHeader>> GetEnumerator()
        {
            return headers.Select(h => new KeyValuePair<string, HttpHeader>(h.Name, h)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool HasName(string name)
        {
            return headers.Where(h => h.Name == name).Any();
        }
    }
}
