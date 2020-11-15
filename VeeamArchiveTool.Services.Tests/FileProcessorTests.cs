using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.IO;

using VeeamArchiveTool.Common.Abstractions;

namespace VeeamArchiveTool.Services.Tests
{
    [TestClass]
    public class FileProcessorTests
    {
        private string _testFilePath;

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
                for (int i = 0; i < 2048 * 128; i++)
                {
                    rng.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var executionContextMock = new Mock<IExecutionContext>();
            FileInfo fileInfo = new FileInfo(_testFilePath);

            //FileProcessor fileProcessor = new FileProcessor();
            //fileProcessor.GetChunks()
            //var sd = "sdf";
        }

        [TestCleanup]
        public void CleanUp()
        {
            File.Delete(_testFilePath);
        }
    }
}
