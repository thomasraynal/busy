using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public class PeerDirectoryServer : IPeerDirectoryServer
    {
        private readonly IPeerRepository _peerRepository;
        private Peer _self;
        private IBus _bus;
        private ILogger _logger;

        public PeerDirectoryServer(ILogger logger, IPeerRepository peerRepository)
        {
            _peerRepository = peerRepository;
            _logger = logger;
        }

        public void Configure(IBus bus)
        {
            _bus = bus;
        }

        public IList<Peer> GetPeersHandlingMessage(IMessage message)
        {
            return GetPeersHandlingMessage(MessageBinding.FromMessage(message));
        }

        public IList<Peer> GetPeersHandlingMessage(MessageBinding messageBinding)
        {
            return _peerRepository.GetPeers()
                                  .Where(peer => peer.Subscriptions != null && peer.Subscriptions.Any(x => x.MessageTypeId == messageBinding.MessageTypeId && x.Matches(messageBinding.RoutingKey)))
                                  .Select(peerDesc => peerDesc.Peer)
                                  .ToList();
        }

        public PeerDescriptor GetPeerDescriptor(PeerId peerId)
        {
            return _peerRepository.Get(peerId);
        }

        public IEnumerable<PeerDescriptor> GetPeerDescriptors()
        {
            return _peerRepository.GetPeers();
        }

        public void AddOrUpdatePeerEntry(PeerDescriptor peerDescriptor)
        {
            _peerRepository.AddOrUpdatePeer(peerDescriptor);
        }

        public Task RegisterAsync(Peer self, IEnumerable<Subscription> subscriptions)
        {
            _self = self;

            var selfDescriptor = new PeerDescriptor(self.Id, self.EndPoint, self.IsUp, self.IsResponding, DateTime.Now, subscriptions.ToArray());

            AddOrUpdatePeerEntry(selfDescriptor);

            _bus.Publish(new PeerActivated(selfDescriptor));

            return Task.CompletedTask;
        }

        public Task UpdateSubscriptionsAsync(IEnumerable<SubscriptionsForType> subscriptionsForTypes)
        {
            var subsForTypes = subscriptionsForTypes.ToList();
            var subscriptionsToAdd = subsForTypes.Where(sub => sub.BindingKeys != null && sub.BindingKeys.Any()).ToArray();
            var subscriptionsToRemove = subsForTypes.Where(sub => sub.BindingKeys == null || !sub.BindingKeys.Any()).ToList();

            if (subscriptionsToAdd.Any())
                _peerRepository.AddDynamicSubscriptionsForTypes(_self.Id, subscriptionsToAdd);

            if (subscriptionsToRemove.Any())
                _peerRepository.RemoveDynamicSubscriptionsForTypes(_self.Id, subscriptionsToRemove.Select(sub => sub.MessageTypeId).ToArray());

            _bus.Publish(new PeerSubscriptionsForTypesUpdated(_self.Id, DateTime.Now, subsForTypes.ToArray()));

            return Task.CompletedTask;
        }

        public Task UnregisterAsync(IBus bus)
        {
            return Task.CompletedTask;
        }

        public void Handle(PeerStarted message)
        {
            AddOrUpdatePeerEntry(message.PeerDescriptor);
            _bus.Publish(new PeerActivated(message.PeerDescriptor));
        }

        public void Handle(PeerStopped message)
        {
            _peerRepository.RemovePeer(message.PeerId);
        }

        public Task UnregisterAsync()
        {
            return Task.CompletedTask;
        }

        public void Handle(UpdatePeerSubscriptionsForTypesCommand message)
        {
            _bus.Publish(new PeerSubscriptionsForTypesUpdated(message.PeerId, message.TimestampUtc, message.SubscriptionsForTypes));
        }

        public void Handle(PeerSubscriptionsForTypesUpdated message)
        {
            _peerRepository.AddDynamicSubscriptionsForTypes(message.PeerId, message.SubscriptionsForType);
        }
    }
}
