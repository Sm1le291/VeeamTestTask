using System;
using System.Collections.Generic;
using System.Threading;

namespace VeeamArchiveTool.Common
{
    public class BlockingQueue<T> : IDisposable
    {
        EventWaitHandle _eventWait = new EventWaitHandle(false, EventResetMode.AutoReset);
        EventWaitHandle _onCompleted  = new EventWaitHandle(false, EventResetMode.AutoReset);
        volatile int _wasClosed = 0;

        readonly object _locker = new object();



        readonly Queue<T> _collection = new Queue<T>();

        public bool IsCompleted
        {
            get
            {
                return _wasClosed > 0 && Count == 0;
            }
        }

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _collection.Count;
                }
            }
        }

        public void Add(T item)
        {
            if (_wasClosed < 1)
            {
                    lock (_locker)
                    {   
                        if (_wasClosed < 1)
                        {
                            _collection.Enqueue(item);
                            _eventWait.Set();
                        }
                        else
                        {
                            ThrowIfCompleted();
                        }
                    }
            }
            else
            {
                ThrowIfCompleted();
            }
        }

        private void ThrowIfCompleted()
        {
            throw new InvalidOperationException("The collection was completed");
        }

        public T Take()
        {
            while (true)
            {
                T item = default(T);

                lock (_locker)
                {
                    if (_collection.Count > 0)
                    {
                        item = _collection.Dequeue();

                        return item;
                    }
                }
                
                int idx = WaitHandle.WaitAny(new[] { _eventWait, _onCompleted });
                
                if (idx == 1)
                {
                    Interlocked.Increment(ref _wasClosed);
                    throw new InvalidOperationException("Queue was closed");
                }

                    
            }
        }

        public void CompleteAdding()
        {
            Interlocked.Increment(ref _wasClosed);
            _onCompleted.Set();
        }

        public void Dispose()
        {
            _eventWait.Close();
        }
    }
}
