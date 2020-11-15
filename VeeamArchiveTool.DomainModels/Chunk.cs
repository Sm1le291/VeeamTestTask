using System;
using System.Collections.Generic;
using System.Text;

namespace VeeamArchiveTool.DomainModels
{
    public class Chunk
    {
        public ChunkOffsetsInfo ChunkOffsetsInfo { get; set; }

        public byte[] Bytes { get; set; }
    }
}
