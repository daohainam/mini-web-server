using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc.Abstraction
{
    public class ViewException : Exception
    {
        public ViewException()
        {
        }

        public ViewException(string? message) : base(message)
        {
        }

        public ViewException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ViewException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
