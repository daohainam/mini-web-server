using MiniWebServer.Abstractions;
using MiniWebServer.MiniApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.MimeType
{
    // this class uses Decorator pattern
    public class MimeTypeMappingContainer : IMimeTypeMapping
    {
        private readonly IEnumerable<IMimeTypeMapping> mappings;

        public MimeTypeMappingContainer(IEnumerable<IMimeTypeMapping> mappings)
        {
            this.mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
        }

        public string GetMimeMapping(string fileExt)
        {
            foreach (var mapping in mappings)
            {
                var mimeType = mapping.GetMimeMapping(fileExt);
                if (mimeType != null)
                {
                    return mimeType;
                }
            }

            return "application/octet-stream";
        }
    }
}
