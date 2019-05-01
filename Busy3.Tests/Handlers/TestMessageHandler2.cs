using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class TestMessageHandler2 : IMessageHandler<TestMessage>
    {
        public void Handle(TestMessage message)
        {
            TestsMessageDispatchingContext.Increment();
        }

        public void Handle(IMessage message)
        {
            Handle(message as TestMessage);
        }
    }
}
