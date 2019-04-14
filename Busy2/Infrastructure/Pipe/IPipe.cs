using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface IPipe
    {
        string Name { get; }
        int Priority { get; }
        bool IsAutoEnabled { get; }

        void BeforeInvoke(BeforeInvokeArgs args);
        void AfterInvoke(AfterInvokeArgs args);
    }
}
