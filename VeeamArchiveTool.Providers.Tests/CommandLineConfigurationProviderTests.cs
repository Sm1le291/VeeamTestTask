using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.IO;

using VeeamArchiveTool.Common.Models;

namespace VeeamArchiveTool.Providers.Tests
{
    [TestClass]
    public class CommandLineConfigurationProviderTests
    {
        private string _randomFilePath;

        private string _gzFilePath;

        private int FILE_SIZE_MB = 1;

        [TestInitialize]
        public void SetUp()
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var testRandomFile = "test1.iso";

            _randomFilePath = Path.Combine(currentDirectory, testRandomFile);

            byte[] data = new byte[8192];
            Random rng = new Random();
            using (FileStream stream = File.OpenWrite(_randomFilePath))
            {
                for (int i = 0; i < FILE_SIZE_MB * 128; i++)
                {
                    rng.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }

            var _testGzFile = "test1.gz";

            _gzFilePath = Path.Combine(currentDirectory, _testGzFile);

            data = new byte[8192];
            rng = new Random();
            using (FileStream stream = File.OpenWrite(_gzFilePath))
            {
                for (int i = 0; i < FILE_SIZE_MB * 128; i++)
                {
                    rng.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        [TestMethod]
        [DataRow("compresswrong", "inputwrongpath", "outputwrongpath")]
        [DataRow("compress", "inputwrongpath", "C:\\test1.gz")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetConfiguration_GeneratesException_ShouldReturnExceptionsInCaseOfWrongParameters(
            string direction,
            string inputPath,
            string outputPath)
        {
            var loggerMock = new Mock<ILogger<CommandLineConfigurationProvider>>();
            CommandLineConfigurationProvider provider = new CommandLineConfigurationProvider(loggerMock.Object);
            provider.GetConfiguration(new string[] { direction, inputPath, outputPath });
        }

        [TestMethod]
        public void GetConfiguration_ReturnsAppConfiguration_ShouldReturnConfigurationIfParametersAreCorrect()
        {
            var direction = "compress";
            var inputFile = _randomFilePath;
            var outputFile = _gzFilePath;

            var expectedResult = new AppConfiguration
            {
                ProcessDirection = ProcessDirection.Compress,
                InputFilePath = inputFile,
                OutputFilePath = outputFile
            };

            var loggerMock = new Mock<ILogger<CommandLineConfigurationProvider>>();
            CommandLineConfigurationProvider provider = new CommandLineConfigurationProvider(loggerMock.Object);

            var actualResult = provider.GetConfiguration(new string[] { direction, inputFile, outputFile });
            Assert.AreEqual(expectedResult.ProcessDirection, actualResult.ProcessDirection);
            Assert.AreEqual(expectedResult.InputFilePath, actualResult.InputFilePath);
            Assert.AreEqual(expectedResult.OutputFilePath, actualResult.OutputFilePath);
        }


        [TestCleanup]
        public void CleanUp()
        {
            File.Delete(_randomFilePath);
            File.Delete(_gzFilePath);
        }
    }
}
