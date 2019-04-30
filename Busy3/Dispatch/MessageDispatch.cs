using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Busy
{
    public class MessageDispatch : IMessageDispatch
    {
        private static readonly object _exceptionsLock = new object();
        private Dictionary<Type, Exception> _exceptions;

        public MessageDispatch(IMessage message)
        {
            Message = message;
        }
        public IMessage Message { get; private set; }

        public void SetHandled(Type messageHandlerType, Exception error)
        {
            if (error != null)
            {
                AddException(messageHandlerType, error);
            }
        }

        private void AddException(Type messageHandlerType, Exception error)
        {
            lock (_exceptionsLock)
            {
                if (_exceptions == null)
                    _exceptions = new Dictionary<Type, Exception>();

                _exceptions[messageHandlerType] = error;
            }
        }

    }
}
