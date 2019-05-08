using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public interface IPeerDirectory : IMessageHandler<PeerStarted>,
                                     IMessageHandler<PeerStopped>,
                                     IMessageHandler<UpdatePeerSubscriptionsForTypesCommand>,
                                     IMessageHandler<PeerSubscriptionsForTypesUpdated>
    {

        Task RegisterAsync(Peer self, IEnumerable<Subscription> subscriptions);
        Task UpdateSubscriptionsAsync(IEnumerable<SubscriptionsForType> subscriptionsForTypes);
        Task UnregisterAsync();

        IList<Peer> GetPeersHandlingMessage(IMessage message);
        IList<Peer> GetPeersHandlingMessage(MessageBinding messageBinding);
        void Configure(IBus bus);
    }
}
