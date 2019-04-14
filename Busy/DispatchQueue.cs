using log4net;
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

        private static readonly ILog _logger = LogManager.GetLogger(typeof(DispatchQueue));

        private FlushableBlockingCollection<Entry> _queue = new FlushableBlockingCollection<Entry>();
        private Thread _thread;
        private volatile bool _isRunning;
        private SynchronizationContext SynchronizationContext { get; }

        public MessageHandlerInvokerMode Mode { get; private set; }
        public bool IsRunning { get; private set; }
        public int QueueLength => _queue.Count;


        public DispatchQueue(MessageHandlerInvokerMode mode)
        {
            Mode = mode;
            SynchronizationContext = new DispatchQueueSynchronizationContext(this);
        }

        public void Dispose()
        {
            Stop();
        }

        public void Enqueue(MessageDispatch dispatch)
        {
            _queue.Add(new Entry(dispatch));
        }

        private void Enqueue(Action action)
        {
            _queue.Add(new Entry(action));
        }

        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _thread = new Thread(ThreadProc)
            {
                IsBackground = true
            };

            _thread.Start();
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            _queue.CompleteAdding();

            _thread.Join();

            _queue = new FlushableBlockingCollection<Entry>();
        }

        private void ThreadProc()
        {
            
            try
            {
                var batch = new Batch();

                foreach (var entries in _queue.GetConsumingEnumerable())
                {
                    ProcessEntries(entries, batch);
                }
            }
            finally
            {
                _currentDispatchQueueName = null;
            }
        }

        private void ProcessEntries(List<Entry> entries, Batch batch)
        {
            foreach (var entry in entries)
            {
                batch.Add(entry);
            }

            RunBatch(batch);
        }

        private void RunAction(Action action)
        {
            try
            {
                SynchronizationContext.SetSynchronizationContext(SynchronizationContext);
                action();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void RunSingle(MessageDispatch dispatch)
        {
            if (!_isRunning)
                return;

            var batch = new Batch(1);
            batch.Add(new Entry(dispatch));

            RunBatch(batch);
        }

        private void RunBatch(Batch batch)
        {
            if (batch.IsEmpty)
                return;

            try
            {
                if (!_isRunning)
                    return;

                switch (Mode)
                {
                    case MessageHandlerInvokerMode.Synchronous:
                        {
                            SynchronizationContext.SetSynchronizationContext(null);

                            foreach (var entry in batch.Entries)
                            {
               
                            }

                            batch.SetHandled(null);

                            break;
                        }

                    case MessageHandlerInvokerMode.Asynchronous:
                        {
                            SynchronizationContext.SetSynchronizationContext(SynchronizationContext);


                            foreach (var entry in batch.Entries)
                            {

                            }

                            //var exception = task.IsFaulted
                            //? task.Exception != null
                            //    ? task.Exception.InnerException
                            //    : new Exception("Task failed")
                            //: null;

                            //if (exception != null)
                            //    _logger.Error(exception);

                            //asyncBatch.SetHandled(exception);

                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                batch.SetHandled(ex);
            }
            finally
            {
                batch.Clear();
               // LocalDispatch.Reset();
            }
        }



        public virtual int Purge()
        {
            var flushedEntries = _queue.Flush();
            return flushedEntries.Count;
        }

        public void RunOrEnqueue(MessageDispatch dispatch)
        {
            if (dispatch.ShouldRunSynchronously)
            {
                RunSingle(dispatch);
            }
            else
            {
                dispatch.BeforeEnqueue();
                Enqueue(dispatch);
            }
        }

        private readonly struct Entry
        {
            public Entry(MessageDispatch dispatch)
            {
                Dispatch = dispatch;
                Action = null;
            }

            public Entry(Action action)
            {
                Dispatch = null;
                Action = action;
            }

            public readonly MessageDispatch Dispatch;
            public readonly Action Action;
        }

        private class Batch
        {
            public readonly List<Entry> Entries;
            public readonly List<IMessage> Messages;

            public Batch(int capacity = 200)
            {
                Entries = new List<Entry>(capacity);
                Messages = new List<IMessage>(capacity);
            }

            public Entry FirstEntry => Entries[0];
            public bool IsEmpty => Entries.Count == 0;

            public void Add(Entry entry)
            {
                Entries.Add(entry);
                Messages.Add(entry.Dispatch.Message);
            }

            public void SetHandled(Exception error)
            {
                foreach (var entry in Entries)
                {
                    entry.Dispatch.SetHandled(error);
                }
            }

            public void Clear()
            {
                Entries.Clear();
                Messages.Clear();
            }

            public Batch Clone()
            {
                var clone = new Batch(Entries.Count);
                clone.Entries.AddRange(Entries);
                clone.Messages.AddRange(Messages);
                return clone;
            }
        }

        private class DispatchQueueSynchronizationContext : SynchronizationContext
        {
            private readonly DispatchQueue _dispatchQueue;

            public DispatchQueueSynchronizationContext(DispatchQueue dispatchQueue)
            {
                _dispatchQueue = dispatchQueue;
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                _dispatchQueue.Enqueue(() => d(state));
            }
        }
    }
}
