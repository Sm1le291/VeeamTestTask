using System;
using System.IO;

using Polly;
using Microsoft.Extensions.Logging;
using Polly.Retry;

using VeeamArchiveTool.Common.Abstractions;
using VeeamArchiveTool.Common;


namespace VeeamArchiveTool.Services
{
    public abstract class BaseProcessor
    {
        private readonly int ATTEMPTS_NUMBER = 30;

        private readonly ILogger _logger;

        private readonly IExceptionHandler _exceptionHandler;

        protected BaseProcessor(IExceptionHandler exceptionHandler, ILogger logger)
        {
            _exceptionHandler = exceptionHandler;
            _logger = logger;
        }

        protected void SafelyCheckThatFileExists(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    _logger.LogCritical("Output file already exists");
                    throw new FileAlreadyExistsException("Output file already exists");
                }
                catch (Exception ex)
                {
                    if (_exceptionHandler.HandleException(ex))
                    {
                        _logger.LogInformation("Nice, file will be overriden. We can continue");
                    }
                    else
                    {
                        throw;
                    }
                }

            }
        }

        protected void SafelyCreateEmptyFile(long fileLength, string path)
        {
            var retryPolicy = GetRetryPolicy();

            retryPolicy.Execute(() =>
            {
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.SetLength(fileLength);
                }
            });
        }

        protected void SafelyCreateFileWithContent(byte[] content, string path)
        {
            var retryPolicy = GetRetryPolicy();

            retryPolicy.Execute(() =>
            {
                using (var fs = new FileStream(path,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Write,
                    content.Length,
                    true))
                {
                    fs.Write(content, 0, content.Length);
                }
            });
        }

        protected void SafelyWriteIntoFile(byte[] content, string path, long startPosition)
        {
            var retryPolicy = GetRetryPolicy();

            retryPolicy.Execute(() =>
            {
                using (var fileStream = new FileStream($"{path}",
                FileMode.Open,
                FileAccess.Write,
                FileShare.Write,
                content.Length, true))
                {
                    fileStream.Position = startPosition;
                    fileStream.Write(content, 0, content.Length);
                }
            });
        }

        protected void SafelyExtendFile(long appendLength, string path)
        {
            var retryPolicy = GetRetryPolicy();

            retryPolicy.Execute(() =>
            {
                using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    fs.SetLength(fs.Length + appendLength);
                }
            });
        }

        private RetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<IOException>()
                .WaitAndRetry(
                ATTEMPTS_NUMBER,
                p => TimeSpan.FromSeconds(2),
                (ex, number, context) => _logger.LogCritical($"Next attempt from {ATTEMPTS_NUMBER} attempts in  2 seconds  {ex.Message}", context.Count));
        }


    }
}
