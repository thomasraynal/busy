using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    [Transient]
    public class PeerResponded : IEvent
    {
        public readonly PeerId PeerId;

        public PeerResponded(PeerId peerId)
        {
            PeerId = peerId;
        }

        public override string ToString() => $"{PeerId} Responded";
    }
}
