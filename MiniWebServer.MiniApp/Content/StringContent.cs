using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.MiniApp.Content
{
    public class StringContent : ByteArrayContent
    {
        public static StringContent Empty { get; } = new StringContent(string.Empty);

        public StringContent(string content, Encoding encoding) : base(encoding.GetBytes(content))
        {
        }

        public StringContent(string content) : this(content, Encoding.UTF8)
        {
        }

    }
}
