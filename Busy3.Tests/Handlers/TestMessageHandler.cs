﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class TestMessageHandler : IMessageHandler<TestMessage>
    {
        public void Handle(TestMessage message)
        {
            TestsMessageDispatchingContext.Increment();
        }

    }
}
