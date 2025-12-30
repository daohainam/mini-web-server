namespace MiniWebServer.MiniApp.Content;

public class EmptyBodyFileContent : FileContent
{
    public EmptyBodyFileContent(string fileName) : base(fileName)
    {
    }
    public EmptyBodyFileContent(FileInfo file) : base(file)
    {
    }

    public override Task<long> WriteToAsync(Stream stream, CancellationToken cancellationToken)
    {
        // we send nothing, this response content is used mainly to serve HEAD requests

        return Task.FromResult(0L);
    }
}
