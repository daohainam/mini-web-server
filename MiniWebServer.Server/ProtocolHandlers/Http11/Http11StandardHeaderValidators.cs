using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MiniWebServer.Abstractions.ProtocolHandlerStates;

namespace MiniWebServer.Server.ProtocolHandlers.Http11
{
    public class Http11StandardHeaderValidators
    {
        public class ContentLengthHeaderValidator : IHeaderValidator
        {
            public bool Validate(string name, string value, Http11ProtocolData stateObject)
            {
                if ("Content-Length".Equals(name))
                {
                    if (long.TryParse(value, out long length))
                    {
                        stateObject.ContentLength = length;
                    }
                    else
                    {
                        return false; // we only return false when it is an error, otherwise we return true to continue processing flow
                    }
                }

                return true;
            }
        }

        public class TransferEncodingHeaderValidator : IHeaderValidator
        {
            
            protected virtual bool IsSupportedEncoding(string encoding)
            {
                return true; // we virtually support everything for now :|
            }
            public bool Validate(string name, string value, Http11ProtocolData stateObject)
            {
                if (!string.IsNullOrEmpty(value) && "Transfer-Encoding".Equals(name))
                {
                    var encodings = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    foreach (var encoding in encodings)
                    {
                        if (!IsSupportedEncoding(encoding))
                        {
                            return false;
                        }
                    }

                    stateObject.TransferEncoding = encodings;
                }

                return true;
            }
        }

        public class FallbackHeadervalidator : IHeaderValidator
        {
            public bool Validate(string name, string value, Http11ProtocolData stateObject)
            {
                return true;
            }
        }
    }
}
