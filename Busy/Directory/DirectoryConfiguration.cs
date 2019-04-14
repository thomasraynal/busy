using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class DirectoryConfiguration : IDirectoryConfiguration
    {
        public DirectoryConfiguration()
        {
            PeerPingInterval = 1.Minute();
            PeerPingTimeout = 5.Minutes();
        }

        public TimeSpan PeerPingInterval { get; private set; }
        public TimeSpan PeerPingTimeout { get; private set; }

    }
}
