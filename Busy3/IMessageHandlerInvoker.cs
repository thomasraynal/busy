using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public interface IMessageHandlerInvoker
    {
        Type MessageHandlerType { get; }
        MessageHandlerInvokerMode Mode { get; }
        Task InvokeMessageHandler(IMessageDispatch dispatch);
    }
}
