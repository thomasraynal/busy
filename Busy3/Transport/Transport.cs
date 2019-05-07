using Busy.Utils;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busy
{
    public class Transport : ITransport
    {
        private IMessageSerializer _serializer;
        private ILogger _logger;
        private ConcurrentDictionary<PeerId, PushSocket> _outboundSockets;
        private BlockingCollection<OutboundSocketAction> _outboundSocketActions;
        private BlockingCollection<PendingDisconnect> _pendingDisconnects;
        private Thread _inboundThread;
        private Thread _outboundThread;
        private Thread _disconnectThread;
        private volatile bool _isListening;

        public Transport(IMessageSerializer serializer, ILogger logger)
        {
            _outboundSockets = new ConcurrentDictionary<PeerId, PushSocket>();
            _serializer = serializer;
            _logger = logger;
        }

        public void Configure(PeerId peerId, string endpoint)
        {
            PeerId = peerId;
            InboundEndPoint = endpoint;
        }

        public PeerId PeerId { get; private set; }

        public string InboundEndPoint { get; private set; }

        public event Action<TransportMessage> MessageReceived;

        public void AckMessage(TransportMessage transportMessage)
        {
            throw new NotImplementedException();
        }

        public void Send(TransportMessage message, IEnumerable<Peer> peers)
        {
            _outboundSocketActions.Add(OutboundSocketAction.Send(message, peers));
        }

        public void Start()
        {
            _outboundSockets = new ConcurrentDictionary<PeerId, PushSocket>();
            _outboundSocketActions = new BlockingCollection<OutboundSocketAction>();
            _pendingDisconnects = new BlockingCollection<PendingDisconnect>();


            _isListening = true;

            _inboundThread = new Thread(InboundProc);
            _outboundThread = new Thread(OutboundProc);
            _disconnectThread = new Thread(DisconnectProc);

            _inboundThread.Start();
            _outboundThread.Start();
            _disconnectThread.Start();

        }

        public void Stop()
        {
            _pendingDisconnects.CompleteAdding();
            _disconnectThread.Join(5.Seconds());

            _outboundSocketActions.CompleteAdding();
            _outboundThread.Join(5.Seconds());

            _isListening = false;

            _inboundThread.Join(5.Seconds());
            _outboundSocketActions.Dispose();

        }

        private void InboundProc()
        {
      
            var inboundSocket =  new PullSocket(InboundEndPoint);

            using (inboundSocket)
            {
                while (_isListening)
                {
                    var inputStream = inboundSocket.ReceiveFrameBytes();

                    DeserializeAndForwardTransportMessage(inputStream);
                }

                inboundSocket.Dispose();
            }

        }


        private void DeserializeAndForwardTransportMessage(byte[] inputStream)
        {

            var transportMessage = _serializer.Deserialize(inputStream, typeof(TransportMessage)) as TransportMessage;

            if (_isListening)
                MessageReceived?.Invoke(transportMessage);

        }

        private void OutboundProc()
        {

            foreach (var socketAction in _outboundSocketActions.GetConsumingEnumerable())
            {
                WriteTransportMessageAndSendToPeers(socketAction.Message, socketAction.Targets);
            }

        }


        private void WriteTransportMessageAndSendToPeers(TransportMessage transportMessage, List<Peer> peers)
        {

            foreach (var target in peers)
            {
                var sender = _outboundSockets.AddOrUpdate(target.Id, (peerId) => new PushSocket(target.EndPoint), (peerId, socket) => socket); var payload = _serializer.Serialize(transportMessage);
                sender.SendFrame(payload);

                Task.Delay(20).Wait();

            }
        }

        private void DisconnectProc()
        {
      
            foreach (var pendingDisconnect in _pendingDisconnects.GetConsumingEnumerable())
            {
                while (pendingDisconnect.DisconnectTimeUtc > DateTime.Now)
                {
                    if (_pendingDisconnects.IsAddingCompleted)
                        return;

                    Thread.Sleep(500);
                }

                _outboundSocketActions.Add(OutboundSocketAction.Disconnect(pendingDisconnect.PeerId));

            }
        }
    }
}
