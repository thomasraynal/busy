using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests.Handlers
{
    public class TestMessageHandler3 : IMessageHandler<TestMessage2>
    {
        public void Handle(TestMessage2 message)
        {
            TestsMessageDispatchingContext2.Increment();
        }
    }
}
