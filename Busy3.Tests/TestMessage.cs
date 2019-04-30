using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class TestMessage : IMessage
    {
        public TestMessage()
        {
            Value = new Random().NextDouble();
        }

        public double Value { get; }
    }
}
