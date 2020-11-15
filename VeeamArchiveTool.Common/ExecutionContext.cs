using System.Threading;

using VeeamArchiveTool.Common.Abstractions;
using VeeamArchiveTool.Common.Models;

namespace VeeamArchiveTool.Common
{
    public class ExecutionContext : IExecutionContext
    {
        public ProcessDirection ProcessDirection { get; set; }

        public long _currentFileOutputSize;

        public long GetCurrentOutputFileSize()
        {
            return Interlocked.Read(ref _currentFileOutputSize);
        }

        public void IncrementOuputFileSize(long numberOfBytes)
        {
            Interlocked.Add(ref _currentFileOutputSize, numberOfBytes);
        }

        public int ChunkSize { get; set; }

        public string InputFilePath { get; set; }

        public string OutputFilePath { get; set; }

        public int FileInformationBlockSizeInBytes { get { return 268; } }
        
        public int ServiceBlockSizeInBytes { get { return 385; } }

        private double _numberOfChunksInInputCompressedFile;

        public void SetNumberOfChunksInInputFile(double number)
        {
            if (ProcessDirection == ProcessDirection.Compress)
            {
                throw new System.Exception("Number of chunks takes from input compressed file  for progress bur purposes, you can't take it from uncompress file");
            }

            _numberOfChunksInInputCompressedFile = number;
        }

        public double GetNumberOfChunksInInputFile()
        {
            if (ProcessDirection == ProcessDirection.Compress)
            {
                throw new System.Exception("Number of chunks takes from input compressed file  for progress bur purposes, you can't take it from uncompress file");
            }

            return _numberOfChunksInInputCompressedFile;
        }
    }
}
