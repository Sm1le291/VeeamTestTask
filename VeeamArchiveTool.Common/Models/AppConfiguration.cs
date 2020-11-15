namespace VeeamArchiveTool.Common.Models
{
    public class AppConfiguration
    {
        public string InputFilePath { get; set; }

        public string OutputFilePath { get; set; }

        public ProcessDirection ProcessDirection { get; set; }

        public override string ToString()
        {
            return $"Input file path: {InputFilePath}; Output file path: {OutputFilePath}; Operation: {ProcessDirection.ToString()}";
        }
    }
}
