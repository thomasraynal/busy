using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    [Transient]
    public sealed class PeerStarted : IEvent
    {
        public readonly PeerDescriptor PeerDescriptor;

        public PeerStarted(PeerDescriptor peerDescriptor)
        {
            PeerDescriptor = peerDescriptor;
        }

        public override string ToString() => $"{PeerDescriptor.Peer} TimestampUtc: {PeerDescriptor.TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}";
    }
}
