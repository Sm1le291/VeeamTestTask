using System;
using System.Collections.Generic;
using System.Text;

namespace VeeamArchiveTool.DomainModels
{
    [Serializable]
    public class OriginalFileInformation
    {
        public long OriginalFileTotalLength { get; set; }

        //Length of chunk can be changed, because of it, important to save original number of chunks for proper work of progress indicator
        public double NumberOfChunks { get; set; }
    }
}
