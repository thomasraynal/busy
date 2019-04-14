using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface IPipeSource
    {
        IEnumerable<IPipe> GetPipes(Type messageHandlerType);
    }
}
