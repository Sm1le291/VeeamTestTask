using System;

namespace VeeamArchiveTool.DomainModels
{
    [Serializable]
    public class ChunkOffsetsInfo
    {
        public long OriginalBeginPosition { get; set; }

        public long OriginalEndPosition { get; set; }

        public long CompressedLength { get; set; }

        public long OriginalLength { get; set; }

        public int ChunkNumber { get; set; }
    }
}
