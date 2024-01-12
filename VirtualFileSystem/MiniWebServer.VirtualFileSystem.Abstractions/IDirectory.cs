using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.VirtualFileSystem.Abstractions
{
    public interface IDirectory: IFileSystemObject
    {
        IEnumerable<IFile> GetFiles(string searchPattern, Action<SearchOptions>? searchOptionsAction = null);
    }
}
