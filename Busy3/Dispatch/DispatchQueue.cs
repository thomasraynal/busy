using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busy
{
    public class DispatchQueue : IDisposable
    {
        [ThreadStatic]
        private static string _currentDispatchQueueName;

        private readonly int _batchSize;
        private readonly ILogger _logger;
        private FlushableBlockingCollection<DispatchQueueEntry> _queue = new FlushableBlockingCollection<DispatchQueueEntry>();
        private Thread _thread;
        private volatile bool _isRunning;
        private int _asyncInvocationsCount;
        private int _hasCompletedAsyncInvocationsSinceLastWait;

        public string Name { get; }

        private bool IsCurrentDispatchQueue => _currentDispatchQueueName == Name;

        internal SynchronizationContext SynchronizationContext { get; }

        public DispatchQueue(ILogger logger, int batchSize, string name)
        {
            _batchSize = batchSize;
            _logger = logger;

            Name = name;
            SynchronizationContext = new DispatchQueueSynchronizationContext(this);
        }

        public bool IsRunning => _isRunning;

        public int QueueLength => _queue.Count;

        public void Dispose()
        {
            Stop();
        }

        public void Enqueue(IMessageDispatch dispatch, IMessageHandlerInvoker invoker)
        {
            _queue.Add(new DispatchQueueEntry(dispatch, invoker));
        }



        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _thread = new Thread(ThreadProc)
            {
                IsBackground = true,
                Name = $"{Name}.DispatchThread",
            };

            _thread.Start();
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            while (WaitUntilAllMessagesAreProcessed())
                Thread.Sleep(1);

            _queue.CompleteAdding();

            _thread.Join();

            _queue = new FlushableBlockingCollection<DispatchQueueEntry>();
        }

        private void ThreadProc()
        {
            _currentDispatchQueueName = Name;
            try
            {
                _logger.LogInformation("{0} processing started", Name);
         

                foreach (var entries in _queue.GetConsumingEnumerable(_batchSize))
                {
                    ProcessEntries(entries);
                }

                _logger.LogInformation("{0} processing stopped", Name);
            }
            finally
            {
                _currentDispatchQueueName = null;
            }
        }

        private void ProcessEntries(List<DispatchQueueEntry> entries)
        {
            RunInternal(entries);
        }

        private void RunSingle(IMessageDispatch dispatch, IMessageHandlerInvoker invoker)
        {
            if (!_isRunning)
                return;


            RunInternal(new List<DispatchQueueEntry> { new DispatchQueueEntry(dispatch, invoker) });
        }

        private void RunInternal(List<DispatchQueueEntry> entries)
        {

            if (!_isRunning)
                return;


            foreach (var entry in entries)
            {
                switch (entry.Invoker.Mode)
                {
                    case MessageHandlerInvokerMode.Synchronous:
                        {
                            SynchronizationContext.SetSynchronizationContext(null);

                            entry.Invoker.InvokeMessageHandler(entry.Dispatch);

                            break;
                        }

                    case MessageHandlerInvokerMode.Asynchronous:
                        {
                            SynchronizationContext.SetSynchronizationContext(SynchronizationContext);

                            entry.Invoker.InvokeMessageHandler(entry.Dispatch);

                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public virtual int Purge()
        {
            var flushedEntries = _queue.Flush();
            return flushedEntries.Count;
        }

        public bool WaitUntilAllMessagesAreProcessed()
        {
            bool continueWait, hasChanged = false;

            do
            {
                continueWait = false;

                while (Volatile.Read(ref _asyncInvocationsCount) > 0)
                {
                    continueWait = true;
                    Thread.Sleep(1);
                }

                if (Interlocked.Exchange(ref _hasCompletedAsyncInvocationsSinceLastWait, 0) != 0)
                    continueWait = true;

                if (_queue.WaitUntilIsEmpty())
                    continueWait = true;

                if (continueWait)
                    hasChanged = true;
            } while (continueWait);

            return hasChanged;
        }

        public void RunOrEnqueue(IMessageDispatch dispatch, IMessageHandlerInvoker invoker)
        {
            if (IsCurrentDispatchQueue)
            {
                RunSingle(dispatch, invoker);
            }
            else
            {
                Enqueue(dispatch, invoker);
            }
        }

        private class DispatchQueueSynchronizationContext : SynchronizationContext
        {
            private readonly DispatchQueue _dispatchQueue;

            public DispatchQueueSynchronizationContext(DispatchQueue dispatchQueue)
            {
                _dispatchQueue = dispatchQueue;
            }

        }
    }
}
