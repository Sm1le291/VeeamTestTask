using System;

namespace VeeamArchiveTool.Services.Abstractions
{
    public interface IWorkService
    {
        void Queue(Action action);
    }
}
