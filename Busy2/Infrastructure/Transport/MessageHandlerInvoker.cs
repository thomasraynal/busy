﻿using StructureMap;
using StructureMap.Pipeline;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Busy.Infrastructure
{
    public abstract class MessageHandlerInvoker : IMessageHandlerInvoker
    {
        private readonly Instance _instance;
        private bool? _isSingleton;
        private IBus _bus;

        [ThreadStatic]
        private static MessageContextAwareBus _dispatchBus;

        protected MessageHandlerInvoker(Type handlerType, Type messageType, bool? shouldBeSubscribedOnStartup = null)
        {
            MessageHandlerType = handlerType;
            DispatchQueueName = DispatchQueueNameScanner.GetQueueName(handlerType);
            MessageType = messageType;
            MessageTypeId = new MessageTypeId(MessageType);
            ShouldBeSubscribedOnStartup = shouldBeSubscribedOnStartup ?? MessageShouldBeSubscribedOnStartup(messageType);

            _instance = CreateConstructorInstance(handlerType);
        }

        public Type MessageHandlerType { get; }
        public Type MessageType { get; }
        public MessageTypeId MessageTypeId { get; }
        public bool ShouldBeSubscribedOnStartup { get; }
        public string DispatchQueueName { get; }
        public virtual MessageHandlerInvokerMode Mode => MessageHandlerInvokerMode.Synchronous;

        public abstract void InvokeMessageHandler(IMessageHandlerInvocation invocation);

        public virtual Task InvokeMessageHandlerAsync(IMessageHandlerInvocation invocation)
        {
            throw new NotSupportedException("InvokeMessageHandlerAsync is not supported in Synchronous mode");
        }

        public virtual bool ShouldHandle(IMessage message)
        {
            return true;
        }

        public virtual bool CanMergeWith(IMessageHandlerInvoker other)
        {
            return false;
        }

        public static bool MessageShouldBeSubscribedOnStartup(Type messageType, Type handlerType)
        {
            return MessageShouldBeSubscribedOnStartup(messageType, GetExplicitSubscriptionMode(handlerType));
        }

        internal static bool MessageShouldBeSubscribedOnStartup(Type messageType, SubscriptionMode? subscriptionMode = null)
        {
            if (subscriptionMode != null)
                return subscriptionMode == SubscriptionMode.Auto;

            return !Attribute.IsDefined(messageType, typeof(Routable));
        }

        internal static SubscriptionMode? GetExplicitSubscriptionMode(Type handlerType)
        {
            return SubscriptionMode.Auto;
        }

        protected object CreateHandler(IContainer container, MessageContext messageContext)
        {
            if (IsHandlerSingleton(container))
                return container.GetInstance(MessageHandlerType);

            _bus = _bus ?? container.GetInstance<IBus>();
            if (_bus == null)
                return container.GetInstance(MessageHandlerType);

            try
            {
                _dispatchBus = new MessageContextAwareBus(_bus, messageContext);
                return container.GetInstance(MessageHandlerType, _instance);
            }
            finally
            {
                _dispatchBus = null;
            }
        }

        private bool IsHandlerSingleton(IContainer container)
        {
            if (_isSingleton == null)
            {
                var model = container.Model?.For(MessageHandlerType);
                _isSingleton = model != null && model.Lifecycle == Lifecycles.Singleton;
            }
            return _isSingleton.Value;
        }

        private static Instance CreateConstructorInstance(Type messageHandlerType)
        {
            var inst = new ConstructorInstance(messageHandlerType);
            inst.Dependencies.Add<IBus>(new LambdaInstance<IBus>("Dispatch IBus", () => _dispatchBus));
            inst.Dependencies.Add<MessageContext>(new LambdaInstance<MessageContext>("Dispatch MessageContext", () => _dispatchBus.MessageContext));
            return inst;
        }

        internal static void ThrowIfAsyncVoid(Type handlerType, MethodInfo handleMethod)
        {
            if (handleMethod.ReturnType == typeof(void) && handleMethod.GetAttribute<AsyncStateMachineAttribute>(true) != null)
                throw new InvalidProgramException($"The message handler {handlerType} has an async void Handle method. If you think there are valid use cases for this, please discuss it with the dev team");
        }
    }
}