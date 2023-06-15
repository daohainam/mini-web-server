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
                return long.Parse(TryGetValueAsString("Content-Length"));
            }
            set
            {
                AddOrUpdate("Content-Length", value.ToString());
            }
        }

        public string Connection
        {
            get
            {
                return TryGetValueAsString("Connection");
            }
            set
            {
                AddOrUpdate("Connection", value);
            }
        }
        public string ContentType
        {
            get
            {
                return TryGetValueAsString("Content-Type");
            }
            set
            {
                AddOrUpdate("Content-Type", value);
            }
        }
        public string ContentEncoding
        {
            get
            {
                return TryGetValueAsString("Content-Encoding");
            }
            set
            {
                AddOrUpdate("Content-Encoding", value);
            }
        }
        private string TryGetValueAsString(string name, string defaultValue = "")
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
