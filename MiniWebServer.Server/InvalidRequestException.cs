namespace MiniWebServer.Server
{
    public class InvalidRequestException : Exception
    {
        public InvalidRequestException()
        {
        }

        public InvalidRequestException(string? message) : base(message)
        {
        }

        public InvalidRequestException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
