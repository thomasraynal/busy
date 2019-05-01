using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class Transport : ITransport
    {
        public PeerId PeerId => throw new NotImplementedException();

        public string InboundEndPoint => throw new NotImplementedException();

        public int PendingSendCount => throw new NotImplementedException();

        public event Action<TransportMessage> MessageReceived;

        public void AckMessage(TransportMessage transportMessage)
        {
            throw new NotImplementedException();
        }

        public void Configure(PeerId peerId)
        {
            throw new NotImplementedException();
        }

        public void OnRegistered()
        {
            throw new NotImplementedException();
        }

        public void Send(TransportMessage message, IEnumerable<Peer> peers)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
