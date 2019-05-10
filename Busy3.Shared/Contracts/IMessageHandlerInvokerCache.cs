using System;
using System.Collections.Generic;
using System.Reflection;

namespace Busy
{
    public interface IMessageHandlerInvokerCache
    {
        IEnumerable<IMessageHandler> GetHandlers(Type messageHandlerType);
        MethodInfo GetMethodInfo(Type handlerType, Type messageHandlerType);
    }
}