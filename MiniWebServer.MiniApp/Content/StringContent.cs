using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Content
{
    public class StringContent : ByteArrayContent
    {
        public static StringContent Empty => new(string.Empty);

        public StringContent(string content, Encoding encoding) : base(encoding.GetBytes(content))
        {
        }

        public StringContent(string content) : this(content, Encoding.UTF8)
        {
        }

        public static StringContent FromValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Empty;
            }
            else
            {
                return new StringContent(value);
            }
        }
    }
}
