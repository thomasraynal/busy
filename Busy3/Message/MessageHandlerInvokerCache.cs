using StructureMap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Busy
{
    public class MessageHandlerInvokerCache : IMessageHandlerInvokerCache
    {

        class MessageHandlerInvokerCacheKey
        {
            public MessageHandlerInvokerCacheKey(Type handlerType, Type messageHandlerType)
            {
                HandlerType = handlerType;
                MessageHandlerType = messageHandlerType;
            }

            public Type HandlerType { get; }
            public Type MessageHandlerType { get;  }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }
            public override int GetHashCode()
            {
                return HandlerType.GetHashCode() ^ MessageHandlerType.GetHashCode();
            }
        }

        private readonly ConcurrentDictionary<MessageHandlerInvokerCacheKey, MethodInfo> _methodInfoCache;
        private readonly ConcurrentDictionary<Type, IEnumerable<IMessageHandler>> _handlerCache;
        private readonly IContainer _container;

        public MessageHandlerInvokerCache(IContainer container)
        {
            _methodInfoCache = new ConcurrentDictionary<MessageHandlerInvokerCacheKey, MethodInfo>();
            _handlerCache = new ConcurrentDictionary<Type, IEnumerable<IMessageHandler>>();
            _container = container;
        }

        public IEnumerable<IMessageHandler> GetHandlers(Type messageHandlerType)
        {
            return _handlerCache.GetOrAdd(messageHandlerType, _container.GetAllInstances(messageHandlerType).Cast<IMessageHandler>());
        }

        public MethodInfo GetMethodInfo(Type handlerType, Type messageHandlerType)
        {
            var key = new MessageHandlerInvokerCacheKey(handlerType, messageHandlerType);

            return _methodInfoCache.GetOrAdd(key, handlerType.GetMethod("Handle", new[] { messageHandlerType }));
        }
    }
}
