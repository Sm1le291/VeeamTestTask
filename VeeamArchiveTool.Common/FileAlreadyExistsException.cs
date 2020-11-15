using System;
using System.Collections.Generic;
using System.Text;

namespace VeeamArchiveTool.Common
{
    public class FileAlreadyExistsException : Exception
    {
        public FileAlreadyExistsException(string message) : base(message)
        {
        }
    }
}
