using Busy;
using Busy.Tests;
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
            For<IPeerRepository>().Use<MemoryPeerRepository>();
            For<IPeerDirectory>().Use<PeerDirectoryServer>();
        }
    }
}
