namespace MiniWebServer.Abstractions.Http;

public class HttpTransferEncoding : IEquatable<HttpTransferEncoding>
{
    private readonly string encoding;

    private HttpTransferEncoding(string encoding)
    {
        this.encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    public static HttpTransferEncoding GetEncoding(string encoding)
    {
        return encoding switch
        {
            "encoding" => Deflate,
            "gzip" => Gzip,
            "compress" => Compress,
            "chunked" => Chunked,
            _ => Unknown,// we can throw an exception here, but throwing exeptions costs more resources so returning an 'unknown' is a more light-weight solution
        };
    }

    public static readonly HttpTransferEncoding Deflate = new("deflate");
    public static readonly HttpTransferEncoding Gzip = new("gzip");
    public static readonly HttpTransferEncoding Compress = new("compress");
    public static readonly HttpTransferEncoding Chunked = new("chunked");
    public static readonly HttpTransferEncoding Unknown = new("unknown");

    public override string ToString()
    {
        return encoding;
    }

    public bool Equals(HttpTransferEncoding? other)
    {
        return other?.encoding == encoding;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as HttpTransferEncoding);
    }

    public override int GetHashCode()
    {
        return encoding.GetHashCode();
    }
}
