using System.Runtime.Serialization;

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
    }
}
