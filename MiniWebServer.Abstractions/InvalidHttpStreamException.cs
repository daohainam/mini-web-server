using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    public class InvalidHttpStreamException : Exception
    {
        public InvalidHttpStreamException()
        {
        }

        public InvalidHttpStreamException(string? message) : base(message)
        {
        }

        public InvalidHttpStreamException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidHttpStreamException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
