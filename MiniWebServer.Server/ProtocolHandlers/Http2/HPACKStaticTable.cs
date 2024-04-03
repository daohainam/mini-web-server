using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Http2
{
    internal class HPACKStaticTable
    {
        // https://httpwg.org/specs/rfc7541.html#static.table.definition

        public const string AUTHORITY = ":authority";
        public const string METHOD = ":method";
        public const string PATH = ":path";
        public const string SCHEME = ":scheme";
        public const string STATUS = ":status";

        public const int TableSize = 61;

        public static readonly HPACKHeader[] DecoderTable =
        [
            CreateHeader(1, AUTHORITY, ""),
            CreateHeader(2, METHOD, "GET"),
            CreateHeader(3, METHOD, "POST"),
            CreateHeader(4, PATH, "/"),
            CreateHeader(5, PATH, "/index.html"),
            CreateHeader(6, SCHEME, "http"),
            CreateHeader(7, SCHEME, "https"),
            CreateHeader(8, STATUS, "200"),
            CreateHeader(9, STATUS, "204"),
            CreateHeader(10, STATUS, "206"),
            CreateHeader(11, STATUS, "304"),
            CreateHeader(12, STATUS, "400"),
            CreateHeader(13, STATUS, "404"),
            CreateHeader(14, STATUS, "500"),
            CreateHeader(15, "accept-charset", ""),
            CreateHeader(16, "accept-encoding", "gzip, deflate"),
            CreateHeader(17, "accept-language", ""),
            CreateHeader(18, "accept-ranges", ""),
            CreateHeader(19, "accept", ""),
            CreateHeader(20, "access-control-allow-origin", ""),
            CreateHeader(21, "age", ""),
            CreateHeader(22, "allow", ""),
            CreateHeader(23, "authorization", ""),
            CreateHeader(24, "cache-control", ""),
            CreateHeader(25, "content-disposition", ""),
            CreateHeader(26, "content-encoding", ""),
            CreateHeader(27, "content-language", ""),
            CreateHeader(28, "content-length", ""),
            CreateHeader(29, "content-location", ""),
            CreateHeader(30, "content-range", ""),
            CreateHeader(31, "content-type", ""),
            CreateHeader(32, "cookie", ""),
            CreateHeader(33, "date", ""),
            CreateHeader(34, "etag", ""),
            CreateHeader(35, "expect", ""),
            CreateHeader(36, "expires", ""),
            CreateHeader(37, "from", ""),
            CreateHeader(38, "host", ""),
            CreateHeader(39, "if-match", ""),
            CreateHeader(40, "if-modified-since", ""),
            CreateHeader(41, "if-none-match", ""),
            CreateHeader(42, "if-range", ""),
            CreateHeader(43, "if-unmodified-since", ""),
            CreateHeader(44, "last-modified", ""),
            CreateHeader(45, "link", ""),
            CreateHeader(46, "location", ""),
            CreateHeader(47, "max-forwards", ""),
            CreateHeader(48, "proxy-authenticate", ""),
            CreateHeader(49, "proxy-authorization", ""),
            CreateHeader(50, "range", ""),
            CreateHeader(51, "referer", ""),
            CreateHeader(52, "refresh", ""),
            CreateHeader(53, "retry-after", ""),
            CreateHeader(54, "server", ""),
            CreateHeader(55, "set-cookie", ""),
            CreateHeader(56, "strict-transport-security", ""),
            CreateHeader(57, "transfer-encoding", ""),
            CreateHeader(58, "user-agent", ""),
            CreateHeader(59, "vary", ""),
            CreateHeader(60, "via", ""),
            CreateHeader(61, "www-authenticate", "")
        ];

        private static HPACKHeader CreateHeader(int staticTableIndex, string name, string value) =>
            new( 
                HPACKHeaderTypes.Static,
                staticTableIndex,
                name,
                value);

        public static HPACKHeader? GetHeader(int staticTableIndex)
        {
            if (staticTableIndex >= 1 && staticTableIndex <= DecoderTable.Length)
            {
                return DecoderTable[staticTableIndex - 1];
            }
            else
            {
                return null;
            }
        }
    }
}
