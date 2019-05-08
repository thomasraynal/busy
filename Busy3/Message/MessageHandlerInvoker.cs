using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{


    public class MessageHandlerInvoker : IMessageHandlerInvoker
    {
        private IMessageHandlerInvokerCache _cache;

        public MessageHandlerInvoker(IMessageHandlerInvokerCache cache, MessageHandlerInvokerMode mode, Type messageHandlerType)
        {
            _cache = cache;

            MessageHandlerType = messageHandlerType;
            Mode = mode;
        }

        public Type MessageHandlerType { get; }

        public MessageHandlerInvokerMode Mode { get; }

        public Task InvokeMessageHandler(IMessageDispatch message)
        {
            var handlers = _cache.GetHandlers(MessageHandlerType);

            foreach (var handler in handlers)
            {
                try
                {
                    var invoker = _cache.GetMethodInfo(handler.GetType());
                    invoker.Invoke(handler, new object[] { message.Message });
                }
                catch (Exception ex)
                {
                    message.SetHandled(MessageHandlerType, ex);
                }
            }

            //refacto
            return Task.CompletedTask;
        }
    }
}
