using VeeamArchiveTool.DomainModels;

namespace VeeamArchiveTool.Services.Abstractions
{
    public interface IGZipProcessor
    {
        void GZipChunk(Chunk chunk);

        void UnGZipChunk(Chunk chunk);
    }
}
