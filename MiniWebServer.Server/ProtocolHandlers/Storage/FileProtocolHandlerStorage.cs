using Microsoft.Extensions.Logging;
using MiniWebServer.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Server.ProtocolHandlers.Storage
{
    public class FileProtocolHandlerStorage : IProtocolHandlerStorage, IDisposable
    {
        private readonly string path;
        private readonly ILogger<ProtocolHandlerStorageManager> logger;
        private string? filePath;
        private FileStream? inputStream;
        private FileStream? outputStream;
        private bool disposedValue;

        public FileProtocolHandlerStorage(string path, ILogger<ProtocolHandlerStorageManager> logger)
        {
            this.path = path ?? throw new ArgumentNullException(nameof(path));
            this.logger = logger;
        }

        public Stream GetReader()
        {
            if (filePath == null)
            {
                throw new InvalidOperationException("GetInputStream should be called after a successful call to GetOutputStream");
            }

            if (inputStream == null)
            {
                logger.LogDebug("Opening file {file}", filePath);
                inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }

            return inputStream;
        }

        public Stream GetWriter()
        {
            if (outputStream == null)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                filePath = Path.Combine(path, Guid.NewGuid().ToString());

                logger.LogDebug("Creating file {file}", filePath);
                outputStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            }

            return outputStream;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (inputStream != null)
                    {
                        inputStream.Close();
                        inputStream.Dispose();
                        inputStream = null;
                    }

                    if (outputStream != null)
                    {
                        outputStream.Close();
                        outputStream.Dispose();
                        outputStream = null;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FileProtocolHandlerStorage()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
