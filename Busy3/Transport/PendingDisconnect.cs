using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class PendingDisconnect
    {
        public readonly PeerId PeerId;
        public readonly DateTime DisconnectTimeUtc;

        public PendingDisconnect(PeerId peerId, DateTime disconnectTimeUtc)
        {
            PeerId = peerId;
            DisconnectTimeUtc = disconnectTimeUtc;
        }
    }
}
