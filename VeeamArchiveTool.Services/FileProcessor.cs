using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using VeeamArchiveTool.Common;
using VeeamArchiveTool.Common.Abstractions;
using VeeamArchiveTool.DomainModels;

namespace VeeamArchiveTool.Services
{
    public class FileProcessor : BaseProcessor
    {
        private int _currentChunk;

        private readonly IThreadPool _threadPool;

        private readonly int _chunkSize;

        private readonly IExecutionContext _executionContext;

        

        private ILogger<FileProcessor> _logger;

        private const int MAX_QUEUE_SIZE = 150;

        public FileProcessor(IExecutionContext executionContext, IThreadPool threadPool, ILogger<FileProcessor> logger, IExceptionHandler exceptionHandler) : base(exceptionHandler, logger, executionContext)
        {
            _executionContext = executionContext;
            _chunkSize = executionContext.ChunkSize;
            _threadPool = threadPool;
            _logger = logger;
        }

        public void GetChunks(FileInfo fileInfo, BlockingQueue<Chunk> chunksCollection)
        {
            if (_executionContext.ProcessDirection == Common.Models.ProcessDirection.Compress)
            {
                GetChunksFromUncompressedFile(fileInfo, chunksCollection);
            }
            else
            {
                GetChunksFromCompressedFile(fileInfo, chunksCollection);
            }
        }

        private void GetChunksFromUncompressedFile(FileInfo fileInfo, BlockingQueue<Chunk> chunksCollection)
        {
            using (FileStream originalFileStream = new FileStream(fileInfo.FullName,
                FileMode.Open))
            {
                var originalFileLength = originalFileStream.Length;

                while (originalFileStream.Position < originalFileLength)
                {

                    var chunkArrSize = GetChunkByteArraySize(originalFileLength);

                    var b = new byte[chunkArrSize];

                    var originalPosition = originalFileStream.Position;

                    originalFileStream.Read(b, 0, chunkArrSize);

                    while (_threadPool.PendingWorkItemCount > MAX_QUEUE_SIZE)
                    {
                        Thread.Yield();
                    }

                    chunksCollection.Add(new Chunk
                    {
                        ChunkOffsetsInfo = new ChunkOffsetsInfo
                        {
                            OriginalBeginPosition = originalPosition,
                            OriginalEndPosition = originalFileStream.Position,
                            OriginalLength = b.Length,
                            ChunkNumber = _currentChunk
                        },
                        Bytes = b
                    });

                    ++_currentChunk;
                }

                chunksCollection.CompleteAdding();
            }
        }

        private void GetChunksFromCompressedFile(FileInfo fileInfo, BlockingQueue<Chunk> chunksCollection)
        {
            using (FileStream originalFileStream = new FileStream(_executionContext.InputFilePath,
                FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                originalFileStream.Position += _executionContext.FileInformationBlockSizeInBytes;

                originalFileStream.Position += 1;

                while (originalFileStream.Position < originalFileStream.Length)
                {
                    var chunkInfoSize = _executionContext.ServiceBlockSizeInBytes;
                    var serializedChunkInfo = new byte[chunkInfoSize];
                    originalFileStream.Read(serializedChunkInfo, 0, chunkInfoSize);

                    var chunkInfo = GetChunkInformation(serializedChunkInfo);
                    var compressedChunk = new byte[chunkInfo.CompressedLength];
                    originalFileStream.Position += 1;

                    var chunkLength = Convert.ToInt32(chunkInfo.CompressedLength);
                    originalFileStream.Read(compressedChunk, 0, chunkLength);
                    originalFileStream.Position += 1;

                    chunksCollection.Add(new Chunk
                    {
                        ChunkOffsetsInfo = chunkInfo,
                        Bytes = compressedChunk
                    });
                }

                chunksCollection.CompleteAdding();
            }
        }

        private ChunkOffsetsInfo GetChunkInformation(byte[] serializedChunksInfo)
        {
            using (var ms = new MemoryStream(serializedChunksInfo))
            {
                IFormatter formatter = new BinaryFormatter();
                
                return (ChunkOffsetsInfo)formatter.Deserialize(ms);
            }
        }

        private int GetChunkByteArraySize(long fileLength)
        {
            checked
            {
                long alreadyProcessedSize = _chunkSize * (long)_currentChunk;
                if ((fileLength - (alreadyProcessedSize)) > _chunkSize)
                    return _chunkSize;

                return Convert.ToInt32(fileLength - alreadyProcessedSize);
            }
        }

        public void CreateOutputFile()
        {
            if (_executionContext.ProcessDirection == Common.Models.ProcessDirection.Compress)
            {
                FileInfo fileInfo = new FileInfo(_executionContext.InputFilePath);

                OriginalFileInformation originalFileInformation = new OriginalFileInformation
                {
                    OriginalFileTotalLength = fileInfo.Length,
                    NumberOfChunks = Math.Round((double)fileInfo.Length / _executionContext.ChunkSize, MidpointRounding.AwayFromZero)
                };

                IFormatter formatter = new BinaryFormatter();

                byte[] serializedFileInformation = null;

                using (MemoryStream str = new MemoryStream())
                {
                    formatter.Serialize(str, originalFileInformation);
                    serializedFileInformation = str.ToArray();
                }

                SafelyCheckThatFileExists(_executionContext.OutputFilePath);

                SafelyCreateFileWithContent(serializedFileInformation, _executionContext.OutputFilePath);

                _executionContext.IncrementOuputFileSize(serializedFileInformation.Length);
            }
            else
            {
                var serviceInfoSize = _executionContext.FileInformationBlockSizeInBytes;

                var serviceInfoBinary = new byte[serviceInfoSize];

                OriginalFileInformation originalFileInformation = null;

                using (FileStream originalFileStream = new FileStream(_executionContext.InputFilePath,
                FileMode.Open))
                {
                    originalFileStream.Read(serviceInfoBinary, 0, serviceInfoSize);
                }

                using (var ms = new MemoryStream(serviceInfoBinary))
                {
                    IFormatter formatter = new BinaryFormatter();
                    originalFileInformation = (OriginalFileInformation)formatter.Deserialize(ms);
                }

                _executionContext.SetNumberOfChunksInInputFile(originalFileInformation.NumberOfChunks);

                SafelyCheckThatFileExists(_executionContext.OutputFilePath);
                SafelyCreateEmptyFile(originalFileInformation.OriginalFileTotalLength, _executionContext.OutputFilePath);
            }
        }
    }
}
