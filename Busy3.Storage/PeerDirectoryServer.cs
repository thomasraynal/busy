using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public class PeerDirectoryServer : IPeerDirectory,
                                      IMessageHandler<PeerStarted>,
                                      IMessageHandler<PeerStopped>,
                                      IMessageHandler<PeerSubscriptionsUpdated>
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

        public Task RegisterAsync(Peer self, IEnumerable<Subscription> subscriptions)
        {
            _self = self;

            var selfDescriptor = new PeerDescriptor(self.Id, self.EndPoint, self.IsUp, self.IsResponding, DateTime.Now, subscriptions.ToArray());

            _peerRepository.AddOrUpdatePeer(selfDescriptor);

            _bus.Publish(new PeerStarted(selfDescriptor));

            return Task.CompletedTask;
        }

        public Task UpdateSubscriptionsAsync(IEnumerable<SubscriptionsForType> subscriptionsForTypes)
        {
            var subsForTypes = subscriptionsForTypes.ToList();
            var subscriptionsToAdd = subsForTypes.Where(sub => sub.BindingKeys != null && sub.BindingKeys.Any()).ToArray();
            var subscriptionsToRemove = subsForTypes.Where(sub => sub.BindingKeys == null || !sub.BindingKeys.Any()).ToList();

            var utcNow = DateTime.Now;
            if (subscriptionsToAdd.Any())
                _peerRepository.AddDynamicSubscriptionsForTypes(_self.Id, utcNow, subscriptionsToAdd);

            if (subscriptionsToRemove.Any())
                _peerRepository.RemoveDynamicSubscriptionsForTypes(_self.Id, utcNow, subscriptionsToRemove.Select(sub => sub.MessageTypeId).ToArray());

            _bus.Publish(new PeerSubscriptionsForTypesUpdated(_self.Id, utcNow, subsForTypes.ToArray()));

            return Task.CompletedTask;
        }

        public Task UnregisterAsync(IBus bus)
        {
            return Task.CompletedTask;
        }

        public void Handle(PeerStarted message)
        {
        }

        public void Handle(PeerStopped message)
        {
        }

        public void Handle(PeerSubscriptionsUpdated message)
        {
        }

        public Task UnregisterAsync()
        {
            return Task.CompletedTask;
        }

        public void Handle(UpdatePeerSubscriptionsForTypesCommand message)
        {
        }

        public void Handle(PeerSubscriptionsForTypesUpdated message)
        {
        }
    }
}
