using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class TestRegistry : Registry
    {
        public TestRegistry()
        {
            For<ILogger>().Use<MockLogger>().Singleton();
            For<IMessageSerializer>().Use<JsonMessageSerializer>();
        }
    }
}
