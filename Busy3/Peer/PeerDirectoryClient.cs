using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Busy
{
    public class PeerDirectoryClient : IPeerDirectoryClient
    {

        private readonly ConcurrentDictionary<MessageTypeId, PeerSubscriptionTree> _globalSubscriptionsIndex = new ConcurrentDictionary<MessageTypeId, PeerSubscriptionTree>();
        private readonly ConcurrentDictionary<PeerId, PeerEntry> _peers = new ConcurrentDictionary<PeerId, PeerEntry>();
        private IBus _bus;
        private ILogger _logger;

        public PeerDirectoryClient(ILogger logger)
        {
            _logger = logger;
        }

        public void Configure(IBus bus)
        {
            _bus = bus;

            CreateDirectoryPeer(_bus.DirectoryEndpoint);
        }

        public IList<Peer> GetPeersHandlingMessage(IMessage message)
        {
            return GetPeersHandlingMessage(MessageBinding.FromMessage(message));
        }

        public IList<Peer> GetPeersHandlingMessage(MessageBinding messageBinding)
        {
            var subscriptionList = _globalSubscriptionsIndex.GetValueOrDefault(messageBinding.MessageTypeId);
            if (subscriptionList == null)
                return Array.Empty<Peer>();

            return subscriptionList.GetPeers(messageBinding.RoutingKey);
        }

        private void CreateDirectoryPeer(string endPoint)
        {
            var peerId = new PeerId("Abc.Zebus.DirectoryService");
            var peer = new Peer(peerId, endPoint);

            var selfDescriptor = new PeerDescriptor(peer.Id, peer.EndPoint, peer.IsUp, peer.IsResponding, DateTime.Now, _bus.AutoSubscribes.ToArray());

            AddOrUpdatePeerEntry(selfDescriptor);
           
        }

        public void AddOrUpdatePeerEntry(PeerDescriptor peerDescriptor)
        {
            var subscriptions = peerDescriptor.Subscriptions ?? Array.Empty<Subscription>();

            _logger.LogInformation($"Register {peerDescriptor}");

            var peerEntry = _peers.AddOrUpdate(peerDescriptor.PeerId,
                                               key => new PeerEntry(peerDescriptor, _globalSubscriptionsIndex),
                                               (key, entry) =>
                                               {
                                                   entry.Peer.EndPoint = peerDescriptor.Peer.EndPoint;
                                                   entry.Peer.IsUp = peerDescriptor.Peer.IsUp;
                                                   entry.Peer.IsResponding = peerDescriptor.Peer.IsResponding;
                                                   entry.TimestampUtc = peerDescriptor.TimestampUtc ?? DateTime.UtcNow;

                                                   return entry;
                                               });

            peerEntry.SetSubscriptions(subscriptions, peerDescriptor.TimestampUtc);
        }

        public void Handle(PeerStopped message)
        {
            _peers.Remove(message.PeerId);
        }

        public void Handle(PeerActivated message)
        {
            AddOrUpdatePeerEntry(message.PeerDescriptor);
        }

        public void Handle(PeerSubscriptionsForTypesUpdated message)
        {
            var peer = _peers.GetValueOrDefault(message.PeerId);
            peer.SetSubscriptionsForType(message.SubscriptionsForType ?? Enumerable.Empty<SubscriptionsForType>(), message.TimestampUtc);
        }

        private PeerDescriptor CreateSelfDescriptor(Peer self, IEnumerable<Subscription> subscriptions)
        {
            return new PeerDescriptor(self.Id, self.EndPoint, true, true, DateTime.Now, subscriptions.ToArray());
        }

        public Task RegisterAsync(Peer self, IEnumerable<Subscription> subscriptions)
        {
            var selfDescriptor = CreateSelfDescriptor(self, subscriptions);

            AddOrUpdatePeerEntry(selfDescriptor);

            _bus.Publish(new PeerStarted(selfDescriptor));

            return Task.CompletedTask;
        }

        public Task UnregisterAsync()
        {
            return Task.CompletedTask;
        }

        public async Task UpdateSubscriptionsAsync(IEnumerable<SubscriptionsForType> subscriptionsForTypes)
        {
            var subscriptions = subscriptionsForTypes as SubscriptionsForType[] ?? subscriptionsForTypes.ToArray();

            if (subscriptions.Length == 0) return;

            var command = new UpdatePeerSubscriptionsForTypesCommand(_bus.PeerId, DateTime.Now, subscriptions);

            foreach (var peer in _peers)
            {
                try
                {
                    var result = await _bus.Send(command);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception");
                }
            }
        }

        public void Handle(UpdatePeerSubscriptionsForTypesCommand message)
        {
            if (message.SubscriptionsForTypes == null || message.SubscriptionsForTypes.Length == 0)
                return;

            _bus.Publish(new PeerSubscriptionsForTypesUpdated(message.PeerId, message.TimestampUtc, message.SubscriptionsForTypes));
        }

        public void Handle(PingPeerCommand message)
        {
            throw new NotImplementedException();
        }
    }
}
