using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public interface IPeerDirectory
    {

        Task RegisterAsync(IBus bus, Peer self, IEnumerable<Subscription> subscriptions);
        Task UpdateSubscriptionsAsync(IBus bus, IEnumerable<SubscriptionsForType> subscriptionsForTypes);
        Task UnregisterAsync(IBus bus);

        IList<Peer> GetPeersHandlingMessage(IMessage message);
        IList<Peer> GetPeersHandlingMessage(MessageBinding messageBinding);
    }
}
