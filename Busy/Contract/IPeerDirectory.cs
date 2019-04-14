using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public interface IPeerDirectory
    {
        event Action<PeerId, PeerUpdateAction> PeerUpdated;

        TimeSpan TimeSinceLastPing { get; }

        Task RegisterAsync(IBus bus, Peer self, IEnumerable<Subscription> subscriptions);
        Task UpdateSubscriptionsAsync(IBus bus, IEnumerable<SubscriptionsForType> subscriptionsForTypes);
        Task UnregisterAsync(IBus bus);

        IList<Peer> GetPeersHandlingMessage(IMessage message);
        IList<Peer> GetPeersHandlingMessage(MessageBinding messageBinding);

        bool IsPersistent(PeerId peerId);
    }
}
