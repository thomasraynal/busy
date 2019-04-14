using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface ITransport
    {
        event Action<TransportMessage> MessageReceived;

        void OnRegistered();
        void OnPeerUpdated(PeerId peerId, PeerUpdateAction peerUpdateAction);

        void Configure(PeerId peerId, string environment);
        void Start();
        void Stop();

        PeerId PeerId { get; }
        string InboundEndPoint { get; }
        int PendingSendCount { get; }

        void Send(TransportMessage message, IEnumerable<Peer> peers, SendContext context);
        void AckMessage(TransportMessage transportMessage);
    }
}
