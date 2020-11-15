using VeeamArchiveTool.Common.Models;

namespace VeeamArchiveTool.Common.Abstractions
{
    public interface IExecutionContext
    {
        ProcessDirection ProcessDirection { get; set; }

        int ChunkSize { get; set; }

        string InputFilePath { get; set; }

        string OutputFilePath { get; set; }

        long GetCurrentOutputFileSize();

        void IncrementOuputFileSize(long numberOfBytes);

        int ServiceBlockSizeInBytes { get; }

        int FileInformationBlockSizeInBytes { get; }

        double GetNumberOfChunksInInputFile();

        void SetNumberOfChunksInInputFile(double number);
    }
}
