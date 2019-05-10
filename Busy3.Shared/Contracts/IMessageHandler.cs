using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public interface IMessageHandler
    {
    }

    public interface IMessageHandler<T> : IMessageHandler where T : class, IMessage
    {
        void Handle(T message);
    }
}
