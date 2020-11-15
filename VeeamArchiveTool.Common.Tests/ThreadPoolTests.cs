using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.Threading;

namespace VeeamArchiveTool.Common.Tests
{
    [TestClass]
    public class ThreadPoolTests
    {
        volatile int _actualValue = 0;
        [TestMethod]
        public void QueueWorkItem_AddWorkItemAndCorrectlyExecute_AddAcceptsTenJobsAndEachOfThemIncreasesCounterByOne()
        {
            int expectedValue = 1000000;

            var mockedLogger = new Mock<ILogger<ThreadPool>>();

            ThreadPool threadPool = new ThreadPool(mockedLogger.Object);

            for (int i = 0; i < 1000000; i++)
            {
                threadPool.QueueWorkItem(() =>
                {
                    Interlocked.Increment(ref _actualValue);
                });
            }

            threadPool.Off();

            Assert.AreEqual(_actualValue, expectedValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void QueueWorkItem_AddWorkItemThrowsException_AddValueThrowsAnExceptionIfThreadPoolWasSwitchedOff()
        {
            var mockedLogger = new Mock<ILogger<ThreadPool>>();

            ThreadPool threadPool = new ThreadPool(mockedLogger.Object);

            threadPool.QueueWorkItem(() => { int x = 1; });
            threadPool.Off();
            threadPool.QueueWorkItem(() => { int x = 1; });
        }
    }
}
