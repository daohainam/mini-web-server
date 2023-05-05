using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Abstractions
{
    /// <summary>
    /// refer to RFC 2046 (https://datatracker.ietf.org/doc/html/rfc2046)
    /// </summary>
    public interface IMimeTypeMapping
    {
        /// <summary>
        /// returns MIME type
        /// </summary>
        /// <param name="fileName">file extension, including the period "."</param>
        /// <returns></returns>
        string GetMimeMapping(string fileExt);
    }
}
