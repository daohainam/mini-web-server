namespace MiniWebServer.Abstractions
{
    public interface IContentWriter
    {
        void Write(ReadOnlySpan<byte> value);
        void Complete();
    }
}
