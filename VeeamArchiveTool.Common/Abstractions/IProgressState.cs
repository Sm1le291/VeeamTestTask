namespace VeeamArchiveTool.Common.Abstractions
{
    public interface IProgressState
    {
        void Initialize();

        void Show(int currentChunk);
    }
}
