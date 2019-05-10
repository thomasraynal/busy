using StructureMap;
using System;

namespace Busy
{
    public interface IMessageDispatch
    {
        IMessage Message { get; }

        void SetHandled(Type messageHandlerType, Exception error);
    }
}