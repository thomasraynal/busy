using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public interface ITransport
    {
        event Action<TransportMessage> MessageReceived;

        void Start();
        void Stop();

        PeerId PeerId { get; }
        string InboundEndPoint { get; }

        void Send(TransportMessage message, IEnumerable<Peer> peers);
        void AckMessage(TransportMessage transportMessage);
    }
}
