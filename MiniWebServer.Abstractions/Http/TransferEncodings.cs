namespace MiniWebServer.Abstractions.Http
{
    public enum TransferEncodings
    {
        None,
        Chunked,
        Compress,
        Deflate,
        Gzip
    }
}
