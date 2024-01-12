namespace MiniWebServer.VirtualFileSystem.Abstractions
{
    public interface IFileSystemObject
    {
        string Name { get; }
        string FullName { get; }
        string Path { get; }
        long Size { get; }
        bool IsDirectory { get; }
        bool IsFile { get; }
    }
}
