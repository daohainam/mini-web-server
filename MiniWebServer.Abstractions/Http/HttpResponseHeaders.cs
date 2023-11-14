namespace MiniWebServer.Abstractions.Http
{
    public class HttpResponseHeaders : HttpHeaders
    {
        public HttpResponseHeaders()
        {
            ContentLength = 0;
        }
        public long ContentLength
        {
            get
            {
                var v = TryGetValueAsString("Content-Length");
                if (v == null)
                    return 0;

                return long.Parse(v);
            }
            set
            {
                AddOrUpdate("Content-Length", value.ToString());
            }
        }

        public string? Connection
        {
            get
            {
                return TryGetValueAsString("Connection");
            }
            set
            {
                ArgumentNullException.ThrowIfNull(nameof(value));
                if (value != null)
                    AddOrUpdate("Connection", value);
            }
        }
        public string? ContentType
        {
            get
            {
                return TryGetValueAsString("Content-Type");
            }
            set
            {
                ArgumentNullException.ThrowIfNull(nameof(value));
                if (value != null)
                    AddOrUpdate("Content-Type", value);
            }
        }
        public string? ContentEncoding
        {
            get
            {
                return TryGetValueAsString("Content-Encoding");
            }
            set
            {
                ArgumentNullException.ThrowIfNull(nameof(value));
                if (value != null)
                    AddOrUpdate("Content-Encoding", value);
            }
        }

        public string? Location
        {
            get
            {
                return TryGetValueAsString("Location");
            }
            set
            {
                ArgumentNullException.ThrowIfNull(nameof(value));
                if (value != null)
                    AddOrUpdate("Location", value);
            }
        }

        private string? TryGetValueAsString(string name, string? defaultValue = null)
        {
            if (TryGetValue(name, out var value))
            {
                if (value == null)
                    return defaultValue;

                return value.Value.FirstOrDefault(defaultValue);
            }
            else
            {
                return defaultValue;
            }

        }
    }
}
