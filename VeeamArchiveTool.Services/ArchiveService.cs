using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Threading;

using VeeamArchiveTool.Common;
using VeeamArchiveTool.Common.Abstractions;
using VeeamArchiveTool.DomainModels;
using VeeamArchiveTool.Services.Abstractions;

namespace VeeamArchiveTool.Services
{
    public class ArchiveService : IArchiveService
    {
        private readonly IExecutionContext _executionContext;

        private readonly IGZipProcessor _gZipProcessor;

        private readonly FileProcessor _fileProcessor;

        private readonly IThreadPool _threadPool;

        private readonly ILogger<ArchiveService> _logger;

        private BlockingQueue<Chunk> _chunks = new BlockingQueue<Chunk>();

        public ArchiveService(
            IThreadPool threadPool,
            IGZipProcessor gZipProcessor,
            FileProcessor fileProcessor,
            IExecutionContext executionContext,
            ILogger<ArchiveService> logger)
        {
            _gZipProcessor = gZipProcessor;
            _fileProcessor = fileProcessor;
            _threadPool = threadPool;
            _executionContext = executionContext;
            _logger = logger;
        }

        public void Process()
        {            
            Thread processChunks = new Thread(ProcessChunks);
            processChunks.Start();

            FileInfo fileInfo = new FileInfo(_executionContext.InputFilePath);

            _fileProcessor.CreateOutputFile();

            _fileProcessor.GetChunks(fileInfo, _chunks);

            processChunks.Join();
            _threadPool.Off();
        }

        private void ProcessChunks()
        {                
            Chunk currentItem = null;


            while (!_chunks.IsCompleted)
            {
                try
                {
                    currentItem = _chunks.Take();
                }
                catch (InvalidOperationException)
                {
                    _logger.LogTrace("Adding of chunks was completed!");
                    break;
                }

                if (_executionContext.ProcessDirection == Common.Models.ProcessDirection.Compress)
                {
                    _threadPool.QueueWorkItem(GZipChunkCaller(currentItem));
                }
                else
                {
                    _threadPool.QueueWorkItem(UnGZipChunkCaller(currentItem));
                }
            }
        }

        private Action GZipChunkCaller(Chunk chunk)
        {
            return () => {
                GZipChunk(chunk);
                chunk = null;
            };
        }

        private void GZipChunk(Chunk chunk)
        {
            _gZipProcessor.GZipChunk(chunk);
        }

        private Action UnGZipChunkCaller(Chunk chunk)
        {
            return () => {
                UnGZipChunk(chunk);
                chunk = null;
            };
        }

        private void UnGZipChunk(Chunk chunk)
        {
            _gZipProcessor.UnGZipChunk(chunk);
        }
    }
}
