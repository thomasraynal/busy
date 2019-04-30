using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class BusConfiguration : IBusConfiguration
    {
        public BusConfiguration(string directoryEndpoint)
        {
            DirectoryEndpoint = directoryEndpoint;
        }

        public string DirectoryEndpoint { get; }
    }
}
