using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    [Transient]
    public class PeerStopped : IEvent
    {
        public readonly PeerId PeerId;

        public readonly string PeerEndPoint;

        public readonly DateTime? TimestampUtc;

        public PeerStopped(Peer peer, DateTime? timestampUtc = null)
            : this(peer.Id, peer.EndPoint, timestampUtc)
        {
        }

        public PeerStopped(PeerId peerId, string peerEndPoint, DateTime? timestampUtc = null)
        {
            PeerId = peerId;
            PeerEndPoint = peerEndPoint;
            TimestampUtc = timestampUtc ?? SystemDateTime.UtcNow;
        }

        public override string ToString() => $"{PeerId} TimestampUtc: {TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}";
    }
}
