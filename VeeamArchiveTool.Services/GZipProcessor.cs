using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using Microsoft.Extensions.Logging;

using VeeamArchiveTool.Common.Abstractions;
using VeeamArchiveTool.DomainModels;
using VeeamArchiveTool.Services.Abstractions;

namespace VeeamArchiveTool.Services
{
    public class GZipProcessor : BaseProcessor, IGZipProcessor
    {
        private readonly IExecutionContext _executionContext;

        private readonly Lazy<IProgressState> _progressState;

        private object _locker = new object();

        public GZipProcessor(IExecutionContext executionContext, Lazy<IProgressState> progressState, IExceptionHandler exceptionHandler, ILogger<GZipProcessor> logger) : base(exceptionHandler, logger)
        {
            _executionContext = executionContext;
            _progressState = progressState;
        }
        private int _chunkCounter = 0;

        public void GZipChunk(Chunk chunk)
        {
            byte[] compressedChunk;

            using (var memoryStream = new MemoryStream())
            using (GZipStream gz = new GZipStream(memoryStream, CompressionMode.Compress, false))
            {
                Interlocked.Increment(ref _chunkCounter);
                gz.Write(chunk.Bytes, 0, chunk.Bytes.Length);
                gz.Close();
                compressedChunk = memoryStream.ToArray();
            }

            chunk.ChunkOffsetsInfo.CompressedLength = compressedChunk.Length;

            IFormatter formatter = new BinaryFormatter();

            byte[] serializedServiceInfo;
            using (MemoryStream str = new MemoryStream())
            {
                formatter.Serialize(str, chunk.ChunkOffsetsInfo);
                serializedServiceInfo = str.ToArray();
            }

            long currentChunkStartWritePosition = 0;

            long totalIncreaseLength = 0;

            lock (_locker)
            {
                currentChunkStartWritePosition = _executionContext.GetCurrentOutputFileSize();

                totalIncreaseLength = chunk.ChunkOffsetsInfo.CompressedLength + serializedServiceInfo.LongLength + 1;
                _executionContext.IncrementOuputFileSize(totalIncreaseLength);

                SafelyExtendFile(totalIncreaseLength, _executionContext.OutputFilePath);
            }

            using (var fileStream = new FileStream( $"{_executionContext.OutputFilePath}",
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.Write,
                Convert.ToInt32(totalIncreaseLength), true))
            {
                fileStream.Position = currentChunkStartWritePosition + 1;
                fileStream.Write(serializedServiceInfo, 0, serializedServiceInfo.Length);

                fileStream.Position += 1;

                fileStream.Write(compressedChunk, 0, compressedChunk.Length);
            }

            _progressState.Value.Show(_chunkCounter);

            GC.Collect(GC.MaxGeneration);
        }

        public void UnGZipChunk(Chunk chunk)
        {
            Interlocked.Increment(ref _chunkCounter);
            byte[] uncompressedChunkContent;

            using (var memoryStream = new MemoryStream(chunk.Bytes))
            using (GZipStream gz = new GZipStream(memoryStream, CompressionMode.Decompress, false))
            using (var resultStream = new MemoryStream())
            {
                gz.CopyTo(resultStream);
                uncompressedChunkContent = resultStream.ToArray();
            }

            if (uncompressedChunkContent.Length != chunk.ChunkOffsetsInfo.OriginalLength)
            {
                throw new Exception("Something went wrong");
            }

            SafelyWriteIntoFile(uncompressedChunkContent, _executionContext.OutputFilePath, chunk.ChunkOffsetsInfo.OriginalBeginPosition);

            _progressState.Value.Show(_chunkCounter);
        }
    }
}
