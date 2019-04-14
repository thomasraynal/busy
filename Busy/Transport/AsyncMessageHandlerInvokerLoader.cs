using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class AsyncMessageHandlerInvokerLoader : MessageHandlerInvokerLoader
    {
        public AsyncMessageHandlerInvokerLoader(IContainer container)
            : base(container, typeof(IAsyncMessageHandler<>))
        {
        }

        protected override IMessageHandlerInvoker BuildMessageHandlerInvoker(Type handlerType, Type messageType, bool shouldBeSubscribedOnStartup)
        {
            return new AsyncMessageHandlerInvoker(Container, handlerType, messageType, shouldBeSubscribedOnStartup);
        }
    }
}
