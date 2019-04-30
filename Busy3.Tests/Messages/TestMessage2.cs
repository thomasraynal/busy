using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class TestMessage2 : IMessage
    {
        public TestMessage2()
        {
            Value = new Random().NextDouble();
        }

        public double Value { get; }
    }
}
