using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    [Transient]
    public sealed class PeerSubscriptionsUpdated : IEvent
    {
        public readonly PeerDescriptor PeerDescriptor;

        public PeerSubscriptionsUpdated(PeerDescriptor peerDescriptor)
        {
            PeerDescriptor = peerDescriptor;
        }

        public override string ToString() => $"PeerId: {PeerDescriptor.PeerId}, TimestampUtc: {PeerDescriptor.TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}";
    }
}
