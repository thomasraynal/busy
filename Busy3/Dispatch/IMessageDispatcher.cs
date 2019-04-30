using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Busy
{
    public interface IMessageDispatcher
    {
        void Dispatch(MessageDispatch dispatch);
        void Dispatch(List<MessageDispatch> dispatchs);
    }
}
