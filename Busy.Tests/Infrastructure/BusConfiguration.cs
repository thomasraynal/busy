using Busy.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class BusConfiguration : IBusConfiguration
    {
        public BusConfiguration(params string[] directoryServiceEndPoints)
        {
            DirectoryServiceEndPoints = directoryServiceEndPoints;
            RegistrationTimeout = 10.Second();
        }

        public string[] DirectoryServiceEndPoints { get; }
        public TimeSpan RegistrationTimeout { get; }
        public TimeSpan StartReplayTimeout => 30.Seconds();
        public bool IsPersistent => false;
        public bool IsDirectoryPickedRandomly => false;
        public bool IsErrorPublicationEnabled => false;
        public int MessagesBatchSize => 200;
    }
}
