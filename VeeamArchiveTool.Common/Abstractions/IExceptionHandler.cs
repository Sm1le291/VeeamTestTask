using System;
using System.Collections.Generic;
using System.Text;

namespace VeeamArchiveTool.Common.Abstractions
{
    public interface IExceptionHandler
    {
        bool HandleException(Exception ex);
    }
}
