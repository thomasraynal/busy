using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public interface IPeerDirectoryConfiguration
    {
        TimeSpan PeerPingInterval { get; }
        TimeSpan PeerPingTimeout { get; }
    }
}

