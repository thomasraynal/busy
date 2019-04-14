using Busy.Infrastructure;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class ZeroMQTransport : ITransport
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ZeroMQTransport));

        private string _inboundEndPoint;

        public ZeroMQTransport(string inboundEndPoint)
        {
            _inboundEndPoint = inboundEndPoint;
        }

        public PeerId PeerId => throw new NotImplementedException();

        public string InboundEndPoint => _inboundEndPoint;

        public int PendingSendCount => throw new NotImplementedException();

        public event Action<TransportMessage> MessageReceived;

        public void AckMessage(TransportMessage transportMessage)
        {

        }

        public void Configure(PeerId peerId, string environment)
        {

        }

        public void OnPeerUpdated(PeerId peerId, PeerUpdateAction peerUpdateAction)
        {
       
        }

        public void OnRegistered()
        {
         
        }

        public void Send(TransportMessage message, IEnumerable<Peer> peers, SendContext context)
        {
            _logger.Info($"SEND : {message}");
        }

        public void Start()
        {
           
        }

        public void Stop()
        {
            
        }
    }
}
