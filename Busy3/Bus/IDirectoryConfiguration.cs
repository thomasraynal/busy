using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public interface IDirectoryConfiguration
    {
        TimeSpan PeerPingInterval { get; }
        TimeSpan PeerPingTimeout { get; }
    }
}

