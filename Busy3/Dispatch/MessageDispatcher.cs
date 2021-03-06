﻿using Microsoft.Extensions.Logging;
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
        private readonly IMessageHandlerInvokerCache _cache;

        public MessageDispatcher(ILogger logger, IContainer container)
        {
            _dispatchQueues = new ConcurrentDictionary<Type, DispatchQueue>();
            _invokers = new ConcurrentDictionary<Type, MessageHandlerInvoker>();
            _logger = logger;
            _cache = container.GetInstance<IMessageHandlerInvokerCache>();
        }
        public void Dispatch(MessageDispatch dispatch)
        {
            var queue = GetQueue(dispatch.Message.GetType());
            var invoker = GetInvoker(dispatch.Message.GetType());

            queue.RunOrEnqueue(dispatch, invoker);

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
                var isAsync = messageType.GetCustomAttributes(true)
                                      .FirstOrDefault(attribute => attribute.GetType() == typeof(AsynchronousAttribute)) != null;

                var mode = isAsync ? MessageHandlerInvokerMode.Asynchronous : MessageHandlerInvokerMode.Synchronous;

                var messageInvoker = new MessageHandlerInvoker(_cache, mode, type);

                return messageInvoker;
            });
        }
    }
}
