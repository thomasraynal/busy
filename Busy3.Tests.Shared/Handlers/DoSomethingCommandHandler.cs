using Busy.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class DoSomethingCommandHandler : IMessageHandler<DoSomething>
    {
        public void Handle(DoSomething message)
        {
            GlobalTestContext.Increment();
        }

    }
}
