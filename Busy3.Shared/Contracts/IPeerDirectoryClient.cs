using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public interface IPeerDirectoryClient : IPeerDirectory, IMessageHandler<PeerActivated>, IMessageHandler<PingPeerCommand>
    {
    }
}
