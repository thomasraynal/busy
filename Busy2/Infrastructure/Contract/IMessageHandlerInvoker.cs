using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy.Infrastructure
{
    public interface IMessageHandlerInvoker
    {
        Type MessageHandlerType { get; }
        Type MessageType { get; }
        MessageTypeId MessageTypeId { get; }
        bool ShouldBeSubscribedOnStartup { get; }
        string DispatchQueueName { get; }
        void InvokeMessageHandler(IMessageHandlerInvocation invocation);
        Task InvokeMessageHandlerAsync(IMessageHandlerInvocation invocation);
        bool ShouldHandle(IMessage message);
        bool CanMergeWith(IMessageHandlerInvoker other);
        MessageHandlerInvokerMode Mode { get; }
    }
}
