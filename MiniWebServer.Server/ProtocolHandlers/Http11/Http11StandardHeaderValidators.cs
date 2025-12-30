using Microsoft.Extensions.Logging;

namespace MiniWebServer.Server.ProtocolHandlers.Http11;

public class Http11StandardHeaderValidators
{
    public class ContentLengthHeaderValidator(long maxLength, ILoggerFactory loggerFactory) : IHeaderValidator
    {
        private readonly ILogger<ContentLengthHeaderValidator> logger = loggerFactory.CreateLogger<ContentLengthHeaderValidator>();

        public bool Validate(string name, IEnumerable<string> value)
        {
            if ("Content-Length".Equals(name))
            {
                if (long.TryParse(value.FirstOrDefault(), out long length))
                {
                    if (length > maxLength)
                    {
                        logger.LogError("Length too long ({length} > {max})", length, maxLength);
                        return false;
                    }
                }
                else
                {
                    return false; // we only return false when it is an error, otherwise we return true to continue processing flow
                }
            }

            return true;
        }
    }

    public class TransferEncodingHeaderValidator(ILoggerFactory loggerFactory) : IHeaderValidator
    {
        private readonly ILogger logger = loggerFactory.CreateLogger(typeof(TransferEncodingHeaderValidator));

        protected virtual bool IsSupportedEncoding(string encoding)
        {
            return true; // we virtually support everything for now :|
        }
        public bool Validate(string name, IEnumerable<string> values)
        {
            string? value = values.FirstOrDefault();

            if (!string.IsNullOrEmpty(value) && "Transfer-Encoding".Equals(name))
            {
                var encodings = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var encoding in encodings)
                {
                    if (!IsSupportedEncoding(encoding))
                    {
                        logger.LogError("Unsupported encoding: {encoding}", encoding);
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class FallbackHeadervalidator : IHeaderValidator
    {
        public bool Validate(string name, IEnumerable<string> value)
        {
            return true;
        }
    }
}
