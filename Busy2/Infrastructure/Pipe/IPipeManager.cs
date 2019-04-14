using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface IPipeManager
    {
        void EnablePipe(string pipeName);
        void DisablePipe(string pipeName);

        PipeInvocation BuildPipeInvocation(IMessageHandlerInvoker messageHandlerInvoker, List<IMessage> messages, MessageContext messageContext);
            
    }
}
