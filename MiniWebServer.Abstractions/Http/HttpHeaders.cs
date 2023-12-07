using System.Collections;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpHeaders : IEnumerable<KeyValuePair<string, IEnumerable<string>>>, IEnumerable<HttpHeader>
    {
        // we have a list of header, but we use a dictionary because we will do a lot of search on this.
        // using a List might save some memories, but slower
        private readonly List<HttpHeader> headers = [];

        public int Count => headers.Count;

        public static bool IsReadOnly => false;


        public delegate void HeaderAddedHandler(HttpHeader header);
        public delegate void HeaderChangedHandler(HttpHeader header);
        public delegate void HeaderRemovedHandler(HttpHeader header);

        public event HeaderAddedHandler? HeaderAdded;
        public event HeaderChangedHandler? HeaderChanged;
        public event HeaderRemovedHandler? HeaderRemoved;

        public HttpHeaders()
        {
        }
        public HttpHeaders(string name, string value)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            Add(name, value);
        }

        public HttpHeaders(params HttpHeader[] headers)
        {
            foreach (var header in headers)
            {
                Add(header);
            }
        }

        public virtual HttpHeaders Add(string name, string value) // we return a HttpHeaders so we can make a chain of call, for example: headers.Add("Connection", "Close").Add("ETag", "ABCD") ...
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            var header = new HttpHeader(name, value);

            headers.Add(header);
            OnHeaderAdded(header);

            return this;
        }

        public virtual HttpHeaders Add(string name, IEnumerable<string> value)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            var header = new HttpHeader(name, value);

            headers.Add(header);
            OnHeaderAdded(header);

            return this;
        }

        public virtual HttpHeaders Add(HttpHeader header)
        {
            headers.Add(header);
            OnHeaderAdded(header);

            return this;
        }

        public virtual HttpHeaders AddOrUpdate(string name, string value)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            var idx = headers.FindIndex(x => x.Name == name);
            if (idx != -1)
            {
                var header = new HttpHeader(name, value);

                headers[idx] = header;
                OnHeaderChanged(header);
            }
            else
            {
                var header = new HttpHeader(name, value);

                headers.Add(header);
                OnHeaderAdded(header);
            }

            return this;
        }

        public virtual HttpHeaders AddOrUpdate(string name, IEnumerable<string> value)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            var idx = headers.FindIndex(x => x.Name == name);
            if (idx != -1)
            {
                var header = new HttpHeader(name, value);

                headers[idx] = header;
                OnHeaderChanged(header);
            }
            else
            {
                var header = new HttpHeader(name, value);

                headers.Add(header);
                OnHeaderAdded(header);
            }

            return this;
        }

        public virtual HttpHeaders AddOrUpdate(HttpHeader header)
        {
            ArgumentNullException.ThrowIfNull(header);

            var idx = headers.FindIndex(x => x.Name == header.Name);
            if (idx != -1)
            {
                headers[idx] = header;
                OnHeaderChanged(header);
            }
            else
            {
                headers.Add(header);
                OnHeaderAdded(header);
            }

            return this;
        }

        public virtual HttpHeaders AddOrUpdate(IEnumerable<HttpHeader> headers)
        {
            ArgumentNullException.ThrowIfNull(headers);

            foreach (var header in headers)
            {
                var idx = this.headers.FindIndex(x => x.Name == header.Name);
                if (idx != -1)
                {
                    this.headers[idx] = header;
                    OnHeaderChanged(header);
                }
                else
                {
                    this.headers.Add(header);
                    OnHeaderAdded(header);
                }

            }

            return this;
        }

        public virtual HttpHeaders AddOrSkip(string name, string value)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            var idx = headers.FindIndex(x => x.Name == name);
            if (idx == -1)
            {
                var header = new HttpHeader(name, value);

                headers.Add(header);
                OnHeaderAdded(header);
            }

            return this;
        }

        public virtual HttpHeaders AddOrSkip(string name, IEnumerable<string> value)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(value);

            var idx = headers.FindIndex(x => x.Name == name);
            if (idx == -1)
            {
                var header = new HttpHeader(name, value);

                headers.Add(header);
                OnHeaderAdded(header);
            }

            return this;
        }

        public virtual HttpHeaders AddOrSkip(HttpHeader header)
        {
            ArgumentNullException.ThrowIfNull(header);

            var idx = headers.FindIndex(x => x.Name == header.Name);
            if (idx == -1)
            {
                headers.Add(header);
                OnHeaderAdded(header);
            }

            return this;
        }

        public virtual HttpHeaders Remove(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            var idx = headers.FindIndex(header => header.Name == name);
            var header = headers[idx];
            headers.RemoveAt(idx);

            OnHeaderRemoved(header);

            return this;
        }

        public bool TryGetValue(string name, out HttpHeader? header)
        {
            ArgumentNullException.ThrowIfNull(name);

            var h = headers.Where(h => name.Equals(h.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (h != null)
            {
                header = h;
                return true;
            }
            else
            {
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

        protected virtual void OnHeaderAdded(HttpHeader header)
        {
            HeaderAdded?.Invoke(header);
        }

        protected virtual void OnHeaderChanged(HttpHeader header)
        {
            HeaderChanged?.Invoke(header);
        }

        protected virtual void OnHeaderRemoved(HttpHeader header)
        {
            HeaderRemoved?.Invoke(header);
        }
    }
}
