using Busy.Handler;
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
        private IContainer _container;

        public MessageHandlerInvoker(IContainer container, MessageHandlerInvokerMode mode, Type messageHandlerType)
        {
            _container = container;
            MessageHandlerType = messageHandlerType;
            Mode = mode;
        }

        public Type MessageHandlerType { get; }

        public MessageHandlerInvokerMode Mode { get; }

        public Task InvokeMessageHandler(IMessageDispatch message)
        {
            var handlers = _container.GetAllInstances(MessageHandlerType).Cast<IMessageHandler>();

            foreach (var handler in handlers)
            {
                try
                {
                    handler.Handle(message.Message);
                }
                catch(Exception ex)
                {
                    message.SetHandled(MessageHandlerType, ex);
                }
            }

            //refacto
            return Task.CompletedTask;
        }
    }
}
