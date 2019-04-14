using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public class SyncMessageHandlerInvokerLoader : MessageHandlerInvokerLoader
    {
        public SyncMessageHandlerInvokerLoader(IContainer container)
            : base(container, typeof(IMessageHandler<>))
        {
        }

        protected override IMessageHandlerInvoker BuildMessageHandlerInvoker(Type handlerType, Type messageType, bool shouldBeSubscribedOnStartup)
        {
            return new SyncMessageHandlerInvoker(Container, handlerType, messageType, shouldBeSubscribedOnStartup);
        }
    }
}
