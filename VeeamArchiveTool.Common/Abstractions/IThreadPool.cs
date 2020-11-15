using System;

namespace VeeamArchiveTool.Common.Abstractions
{
    public interface IThreadPool
    {
        void QueueWorkItem(Action task);

        int PendingWorkItemCount { get; }

        void Off();
    }
}
