using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public partial class PeerDirectoryClient : IPeerDirectory,
                                                  IMessageHandler<PeerStarted>,
                                                  IMessageHandler<PeerStopped>,
                                                  IMessageHandler<PeerSubscriptionsUpdated>
    {

        private readonly ConcurrentDictionary<MessageTypeId, PeerSubscriptionTree> _globalSubscriptionsIndex = new ConcurrentDictionary<MessageTypeId, PeerSubscriptionTree>();
        private readonly ConcurrentDictionary<PeerId, PeerEntry> _peers = new ConcurrentDictionary<PeerId, PeerEntry>();
  
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

        private void AddOrUpdatePeerEntry(PeerDescriptor peerDescriptor)
        {
            var subscriptions = peerDescriptor.Subscriptions ?? Array.Empty<Subscription>();

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
            throw new NotImplementedException();
        }

        public void Handle(PeerStarted message)
        {
            AddOrUpdatePeerEntry(message.PeerDescriptor);
        }

        public void Handle(PeerSubscriptionsUpdated message)
        {
            throw new NotImplementedException();
        }

        private PeerDescriptor CreateSelfDescriptor(Peer self, IEnumerable<Subscription> subscriptions)
        {
            return new PeerDescriptor(self.Id, self.EndPoint, true, true, DateTime.Now, subscriptions.ToArray());
        }

        public Task RegisterAsync(IBus bus, Peer self, IEnumerable<Subscription> subscriptions)
        {
            var selfDescriptor = CreateSelfDescriptor(self, subscriptions);
            AddOrUpdatePeerEntry(selfDescriptor);

            return Task.CompletedTask;
        }

        public Task UnregisterAsync(IBus bus)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateSubscriptionsAsync(IBus bus, IEnumerable<SubscriptionsForType> subscriptionsForTypes)
        {
            var subscriptions = subscriptionsForTypes as SubscriptionsForType[] ?? subscriptionsForTypes.ToArray();

            if (subscriptions.Length == 0) return;


            var self = _peers.First(peer => peer.Key == bus.PeerId);


            self.Value.SetSubscriptionsForType(subscriptionsForTypes, DateTime.Now);

            //var command = new UpdatePeerSubscriptionsForTypesCommand(_self.Id, _timestampProvider.NextUtcTimestamp(), subscriptions);

            //foreach (var peer in _peers)
            //{
            //    try
            //    {
            //        peer.Value.SetSubscriptions()

            //        await bus.Send(command, directoryPeer).WithTimeoutAsync(_configuration.RegistrationTimeout).ConfigureAwait(false);
            //        return;
            //    }
            //    catch (TimeoutException ex)
            //    {
            //        _logger.Error(ex);
            //    }
            //}

            //throw new TimeoutException("Unable to update peer subscriptions on directory");
        }
    }
}
