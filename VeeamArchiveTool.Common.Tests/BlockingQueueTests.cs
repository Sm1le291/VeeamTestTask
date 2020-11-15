

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Threading;

namespace VeeamArchiveTool.Common.Tests
{
    [TestClass]
    public class BlockingQueueTests
    {
        [TestMethod]
        public void Take_AddTwoValues_TakeShouldGetTheSameElementsWhichWeAdded()
        {
            BlockingQueue<int> blockingQueue = new BlockingQueue<int>();
            int[] values = new int[] { 12, 14 };
            List<int> expectedValues = new List<int>();
            int actualNumber = 0;

            int expectedNumber = 2;

            Thread consume = new Thread(() =>
            {
                while (!blockingQueue.IsCompleted)
                {
                    try
                    {
                        var item = blockingQueue.Take();
                        actualNumber++;
                    }
                    catch (InvalidOperationException)
                    {
                        //it means that all values were taken already 
                    }
                }
            });

            consume.Start();


            foreach (var item in values)
            {
                blockingQueue.Add(item);
            }

            blockingQueue.CompleteAdding();


            consume.Join();

            Assert.AreEqual(expectedNumber, actualNumber);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Add_AddValue_AddShouldThrowsANExceptionIfQueueWasClosed()
        {
            BlockingQueue<int> blockingQueue = new BlockingQueue<int>();

            blockingQueue.Add(1);
            blockingQueue.Add(12);

            blockingQueue.CompleteAdding();
            blockingQueue.Add(3);

        }
    }
}