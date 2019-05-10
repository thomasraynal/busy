using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public interface IPeerRepository
    {
        PeerDescriptor Get(PeerId peerId);
        IEnumerable<PeerDescriptor> GetPeers(bool loadDynamicSubscriptions = true);

        void AddOrUpdatePeer(PeerDescriptor peerDescriptor);
        void RemovePeer(PeerId peerId);
        void SetPeerResponding(PeerId peerId, bool isResponding);

        void AddDynamicSubscriptionsForTypes(PeerId peerId, SubscriptionsForType[] subscriptionsForTypes);
        void RemoveDynamicSubscriptionsForTypes(PeerId peerId, MessageTypeId[] messageTypeIds);
        void RemoveAllDynamicSubscriptionsForPeer(PeerId peerId);
    }
}
