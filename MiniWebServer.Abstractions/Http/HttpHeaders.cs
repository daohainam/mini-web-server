using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpHeaders : IEnumerable<KeyValuePair<string, IEnumerable<string>>>, IEnumerable<HttpHeader>
    {
        // we have a list of header, but we use a dictionary because we will do a lot of search on this.
        // using a List might save some memories, but slower
        private readonly List<HttpHeader> headers = new();

        public int Count => headers.Count;

        public bool IsReadOnly => false;

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

        public HttpHeaders AddOrUpdate(string name, string value)
        {
            var idx = headers.FindIndex(x => x.Name == name);
            if (idx != -1)
            {
                headers[idx] = new HttpHeader(name, value);
            }
            else
            {
                headers.Add(new HttpHeader(name, value));
            }

            return this;
        }

        public HttpHeaders AddOrUpdate(string name, IEnumerable<string> value)
        {
            var idx = headers.FindIndex(x => x.Name == name);
            if (idx != -1)
            {
                headers[idx] = new HttpHeader(name, value);
            }
            else
            {
                headers.Add(new HttpHeader(name, value));
            }

            return this;
        }

        public HttpHeaders AddOrUpdate(HttpHeader header)
        {
            var idx = headers.FindIndex(x => x.Name == header.Name);
            if (idx != -1)
            {
                headers[idx] = header;
            }
            else
            {
                headers.Add(header);
            }

            return this;
        }

        public HttpHeaders AddOrSkip(string name, string value)
        {
            var idx = headers.FindIndex(x => x.Name == name);
            if (idx == -1)
            {
                headers.Add(new HttpHeader(name, value));
            }

            return this;
        }

        public HttpHeaders AddOrSkip(string name, IEnumerable<string> value)
        {
            var idx = headers.FindIndex(x => x.Name == name);
            if (idx == -1)
            {
                headers.Add(new HttpHeader(name, value));
            }

            return this;
        }

        public HttpHeaders AddOrSkip(HttpHeader header)
        {
            var idx = headers.FindIndex(x => x.Name == header.Name);
            if (idx == -1)
            {
                headers.Add(header);
            }

            return this;
        }

        public HttpHeaders Remove(string headerName)
        {
            var idx = headers.FindIndex(header => header.Name == headerName);
            headers.RemoveAt(idx);

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

        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
        {
            var list = headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Name, h.Value));
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return headers.GetEnumerator();
        }

        IEnumerator<HttpHeader> IEnumerable<HttpHeader>.GetEnumerator()
        {
            return headers.GetEnumerator();
        }
    }
}
