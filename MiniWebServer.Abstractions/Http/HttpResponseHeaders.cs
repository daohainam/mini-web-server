using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions.Http
{
    public class HttpResponseHeaders: HttpHeaders
    {
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
