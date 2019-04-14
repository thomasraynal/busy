using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface IMessageHandlerInvocation
    {
        IList<IMessage> Messages { get; }
        MessageContext Context { get; }

        IDisposable SetupForInvocation();
        IDisposable SetupForInvocation(object messageHandler);
    }
}
