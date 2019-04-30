using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Busy
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly ConcurrentDictionary<Type, DispatchQueue> _dispatchQueues;
        private readonly ConcurrentDictionary<Type, MessageHandlerInvoker> _invokers;
        private readonly ILogger _logger;
        private readonly IContainer _container;

        public MessageDispatcher(ILogger logger, IContainer container)
        {
            _dispatchQueues = new ConcurrentDictionary<Type, DispatchQueue>();
            _invokers = new ConcurrentDictionary<Type, MessageHandlerInvoker>();
            _logger = logger;
            _container = container;
        }
        public void Dispatch(MessageDispatch dispatch)
        {
            var queue = GetQueue(dispatch.Message.GetType());
            var invoker = GetInvoker(dispatch.Message.GetType());

            queue.RunOrEnqueue(dispatch, invoker);

        }

        private DispatchQueue GetQueue(Type messageType)
        {
            return _dispatchQueues.GetOrAdd(messageType, (key) =>
            {
                var dispatchQueue = new DispatchQueue(_logger, 50, key.Name);
                dispatchQueue.Start();

                return dispatchQueue;

            });
        }

        private MessageHandlerInvoker GetInvoker(Type messageType)
        {
            return _invokers.GetOrAdd(messageType, (key) =>
            {
                var type = typeof(IMessageHandler<>).MakeGenericType(messageType);

                var messageInvoker = new MessageHandlerInvoker(_container, MessageHandlerInvokerMode.Synchronous, type);

                return messageInvoker;
            });
        }

        public void Dispatch(List<MessageDispatch> dispatchs)
        {
            var groups = dispatchs.GroupBy(dispatch => dispatch.Message.GetType());

            foreach (var messages in groups)
            {

                var queue = GetQueue(messages.Key);
                var invoker = GetInvoker(messages.Key);

                foreach (var message in messages)
                {
                    queue.RunOrEnqueue(message, invoker);
                }

            }
        }
    }
}
