using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeeamArchiveTool.DomainModels
{
    public class WriteState
    {
        public FileStream FileStream { get; set; }

        public long StartPosition { get; set; }

        public byte[] Chunk { get; set; }
    }
}
