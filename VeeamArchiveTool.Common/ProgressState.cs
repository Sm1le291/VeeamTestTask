using System;
using System.IO;

using VeeamArchiveTool.Common.Abstractions;

namespace VeeamArchiveTool.Common
{
    public class ProgressState : IProgressState
    {
        private readonly IExecutionContext _executionContext;

        private object _locker = new object();

        private double _chunksPerPercent = 0.0;
        public ProgressState(IExecutionContext executionContext)
        {
            _executionContext = executionContext;
        }

        public void Initialize()
        {
            FileInfo fileInfo = new FileInfo(_executionContext.InputFilePath);

            double numberOfChunks = 0.0;

            if (_executionContext.ProcessDirection == Models.ProcessDirection.Compress)
                numberOfChunks = Math.Round((double)fileInfo.Length / _executionContext.ChunkSize, MidpointRounding.AwayFromZero);
            else
                numberOfChunks = _executionContext.GetNumberOfChunksInInputFile();

            _chunksPerPercent = numberOfChunks / 100.0;
        }


        public void Show(int currentChunk)
        {
            var topPosition = Console.CursorTop;

            lock (_locker)
            {
                Console.SetCursorPosition(1, topPosition);

                double result = currentChunk / _chunksPerPercent;

                result = result > 100 ? 100 : result;

                Console.Write($"Progress {String.Format("{0:0.00}", result)} %");
            }
        }
    }
}
