using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Busy
{
    public class OutboundSocketAction
    {
        private static readonly TransportMessage _disconnectMessage = new TransportMessage(default, null, new PeerId(), null);

        private OutboundSocketAction(TransportMessage message, IEnumerable<Peer> targets)
        {
            Message = message;
            Targets = targets as List<Peer> ?? targets.ToList();
        }

        public bool IsDisconnect => Message == _disconnectMessage;

        public TransportMessage Message { get; }
        public List<Peer> Targets { get; }

        public static OutboundSocketAction Send(TransportMessage message, IEnumerable<Peer> peers)
            => new OutboundSocketAction(message, peers);

        public static OutboundSocketAction Disconnect(PeerId peerId)
            => new OutboundSocketAction(_disconnectMessage, new List <Peer> { new Peer(peerId, null) });
    }
}
