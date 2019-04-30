using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Handler
{
    public interface IMessageHandler
    {
        void Handle(IMessage message);
    }

    public interface IMessageHandler<T> : IMessageHandler where T : class, IMessage
    {
        void Handle(T message);
    }
}
