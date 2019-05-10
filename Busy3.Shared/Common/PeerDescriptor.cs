using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Busy
{
    public class PeerDescriptor
    {
        public Peer Peer { get; set; }

        public Subscription[] Subscriptions { get; set; }

        public DateTime? TimestampUtc { get; set; }

        public PeerDescriptor(PeerId id, string endPoint, bool isUp, bool isResponding, DateTime timestampUtc, params Subscription[] subscriptions)
        {
            Peer = new Peer(id, endPoint, isUp, isResponding);
            PeerId = Peer.Id;
            Subscriptions = subscriptions;
            TimestampUtc = timestampUtc;
        }

        public PeerDescriptor(PeerDescriptor other)
        {
            Peer = new Peer(other.Peer);
            PeerId = Peer.Id;
            Subscriptions = other.Subscriptions?.ToArray() ?? Array.Empty<Subscription>();
            TimestampUtc = other.TimestampUtc;
        }

        private PeerDescriptor()
        {
        }

        public PeerId PeerId { get; set; }


        public override string ToString() => Peer.ToString();
    }
}
