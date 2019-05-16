using Busy;
using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy3.Demo.Shared
{
    public class DemoRegistry : Registry
    {
        public DemoRegistry()
        {
            For<ILogger>().Use<DemoLogger>().Singleton();
            For<IMessageSerializer>().Use<JsonMessageSerializer>();
        }
    }
}
