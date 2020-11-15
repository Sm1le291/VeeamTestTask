using System;

namespace VeeamArchiveTool.Services
{
    [Serializable]
    public class FileChunkInfo
    {
        public long BeginPosition { get; set; }

        public long EndPosition { get; set; }
    }
}
