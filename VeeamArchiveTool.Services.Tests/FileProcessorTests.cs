using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.IO;

using VeeamArchiveTool.Common;
using VeeamArchiveTool.Common.Abstractions;

namespace VeeamArchiveTool.Services.Tests
{
    [TestClass]
    public class FileProcessorTests
    {
        private string _testFilePath;

        private const int FILE_SIZE_MB = 2048;

        private const int CHUNK_SIZE_BYTE = 1024 * 1024 * 5;

        [TestInitialize]
        public void SetUp()
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var testFile = "test1.iso";

            _testFilePath = Path.Combine(currentDirectory, testFile);

            byte[] data = new byte[8192];
            Random rng = new Random();
            using (FileStream stream = File.OpenWrite(_testFilePath))
            {
                for (int i = 0; i < FILE_SIZE_MB * 128; i++)
                {
                    rng.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetChunks_ReturnChunksFromInputFile_ShouldReturnNumberOfChunksEquiualentFileLengthDivideToChunkSize()
        {
            var executionContextMock = new Mock<IExecutionContext>();
            executionContextMock.Setup(x => x.ChunkSize).Returns(CHUNK_SIZE_BYTE);
            executionContextMock.Setup(x => x.ProcessDirection).Returns(Common.Models.ProcessDirection.Compress);
            var exceptionHandlerMock = new Mock<IExceptionHandler>();
            var loggerMock = new Mock<ILogger<FileProcessor>>();
            var threadPool = new ThreadPool(new Mock<ILogger<ThreadPool>>().Object);
            FileInfo fileInfo = new FileInfo(_testFilePath);
            var blockingCollection = new BlockingQueue<DomainModels.Chunk>();

            var expectedCount = 410; // FILE_SIZE_MB / CHUNK_SIZE_BYTE and round to upper

            FileProcessor fileProcessor = new FileProcessor(executionContextMock.Object, threadPool, loggerMock.Object, exceptionHandlerMock.Object);
            fileProcessor.GetChunks(fileInfo, blockingCollection);
            var actualCount = blockingCollection.Count;
            blockingCollection = null;

            Assert.AreEqual(expectedCount, actualCount);
        }

        [TestCleanup]
        public void CleanUp()
        {
            File.Delete(_testFilePath);
        }
    }
}
