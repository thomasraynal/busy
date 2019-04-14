using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Busy.Infrastructure
{
    public interface IMessageHandlerInvokerLoader
    {
        IEnumerable<IMessageHandlerInvoker> LoadMessageHandlerInvokers(params Assembly[] assemblies);
    }
}
