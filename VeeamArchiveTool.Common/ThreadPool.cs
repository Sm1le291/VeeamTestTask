using System;
using System.Collections.Generic;
using System.Threading;

using Microsoft.Extensions.Logging;

using VeeamArchiveTool.Common.Abstractions;

namespace VeeamArchiveTool.Common
{
    public class ThreadPool : IThreadPool
    {
        private readonly ILogger<ThreadPool> _logger;
        private readonly object _locker = new object();
        private readonly Thread[] _workers;
        private volatile int _wasClosed = 0;

        private readonly Queue<Action> _tasks = new Queue<Action>();

        public ThreadPool(ILogger<ThreadPool> logger)
        {
            _logger = logger;

            var concurrencyLevel = Environment.ProcessorCount;

            _workers = new Thread[concurrencyLevel];

            for (int i = 0; i < concurrencyLevel; i++)
            {
                _workers[i] = new Thread(Consume);
                _workers[i].Name = $"Worker {i}";
                _workers[i].Start();
                _logger.LogTrace($"Worker {i} started and is in state {_workers[i].ThreadState}");
            }
        }

        public int PendingWorkItemCount { 
            get 
            {
                lock (_tasks)
                {
                    return _tasks.Count;
                }
            }
        }

        public void QueueWorkItem(Action task)
        {        
            if (_wasClosed < 1)
            {
                lock (_locker)
                {
                    if (_wasClosed < 1)
                    {
                        if (task == null)
                        {
                            var errorMessage = "Task can't be null";
                            _logger.LogError(errorMessage);
                            throw new ArgumentException(errorMessage);
                        }

                        if (_tasks.Count > 0 && _tasks.Peek() == null)
                        {
                            ThrowCantAccept();
                        }

                        EnqueueTaskInternal(task);
                    }
                    else
                    {
                        ThrowCantAccept();
                    }
                }
            }

            else
            {
                ThrowCantAccept();
            }
            
        }

        private void ThrowCantAccept()
        {
            var errorMessage = "ThreadPool can't accept new tasks";
            _logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }

        private void EnqueueTaskInternal(Action task)
        {
            lock (_locker)
            {
                _tasks.Enqueue(task);
                Monitor.Pulse(_locker);
            }
        }

        private void Consume()
        {
            while (true)
            {
                Action item;

                lock (_locker)
                {
                    while(_tasks.Count == 0)
                    {
                        Monitor.Wait(_locker);
                    }

                    item = _tasks.Dequeue();
                }

                if (item == null)
                    break;

                item();
            }

            var currentThread = Thread.CurrentThread;

            _logger.LogTrace($"{currentThread.Name} has been terminated");
        }

        public void Off()
        {
            foreach (var worker in _workers)
            {
                EnqueueTaskInternal(null);
            }

            Interlocked.Increment(ref _wasClosed);

            foreach (var worker in _workers)
            {
                worker.Join();
            }
        }

        public void Dispose()
        {
            Off();
        }
    }
}
