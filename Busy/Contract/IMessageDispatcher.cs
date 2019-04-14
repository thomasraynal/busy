using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Busy
{
    public interface IMessageDispatcher
    {

        IEnumerable<MessageTypeId> GetHandledMessageTypes();

        void Dispatch(MessageDispatch dispatch);
        void Dispatch(MessageDispatch dispatch, Func<Type, bool> handlerFilter);

        void Stop();
        void Start();
        int Purge();
    }
}
