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

        private readonly ConcurrentDictionary<Type, MethodInfo> _methodInfoCache;
        private readonly ConcurrentDictionary<Type, IEnumerable<IMessageHandler>> _handlerCache;
        private readonly IContainer _container;

        public MessageHandlerInvokerCache(IContainer container)
        {
            _methodInfoCache = new ConcurrentDictionary<Type, MethodInfo>();
            _handlerCache = new ConcurrentDictionary<Type, IEnumerable<IMessageHandler>>();
            _container = container;
        }

        public IEnumerable<IMessageHandler> GetHandlers(Type messageHandlerType)
        {
            return _handlerCache.GetOrAdd(messageHandlerType, _container.GetAllInstances(messageHandlerType).Cast<IMessageHandler>());
        }

        public MethodInfo GetMethodInfo(Type handlerType)
        {
            return _methodInfoCache.GetOrAdd(handlerType, handlerType.GetMethod("Handle"));
        }
    }
}
