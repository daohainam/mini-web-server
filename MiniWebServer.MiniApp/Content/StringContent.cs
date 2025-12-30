using System.Text;

namespace MiniWebServer.MiniApp.Content;

public class StringContent(string content, Encoding encoding) : ByteArrayContent(encoding.GetBytes(content))
{
    public static StringContent Empty => new(string.Empty);

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
