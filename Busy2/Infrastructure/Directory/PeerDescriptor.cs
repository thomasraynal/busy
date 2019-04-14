using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Busy.Infrastructure
{
    public class PeerDescriptor
    {
        public readonly Peer Peer;

        public Subscription[] Subscriptions { get; set; }

        public bool IsPersistent { get; set; }

        public DateTime? TimestampUtc { get; set; }

        public bool HasDebuggerAttached { get; set; }

        public PeerDescriptor(PeerId id, string endPoint, bool isPersistent, bool isUp, bool isResponding, DateTime timestampUtc, params Subscription[] subscriptions)
        {
            Peer = new Peer(id, endPoint, isUp, isResponding);
            Subscriptions = subscriptions;
            IsPersistent = isPersistent;
            TimestampUtc = timestampUtc;
        }

        internal PeerDescriptor(PeerDescriptor other)
        {
            Peer = new Peer(other.Peer);
            Subscriptions = other.Subscriptions?.ToArray() ?? Array.Empty<Subscription>();
            IsPersistent = other.IsPersistent;
            TimestampUtc = other.TimestampUtc;
            HasDebuggerAttached = other.HasDebuggerAttached;
        }

        private PeerDescriptor()
        {
        }

        public PeerId PeerId => Peer.Id;

        public override string ToString() => Peer.ToString();
    }
}
