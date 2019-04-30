using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class DispatchQueueEntry
    {
        public DispatchQueueEntry(IMessageDispatch dispatch, IMessageHandlerInvoker invoker)
        {
            Dispatch = dispatch;
            Invoker = invoker;
        }

        public IMessageDispatch Dispatch { get; }
        public IMessageHandlerInvoker Invoker { get; }
    }
}
