using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    [Transient]
    public sealed class PeerSubscriptionsForTypesUpdated : IEvent
    {
        public readonly PeerId PeerId;

        public readonly SubscriptionsForType[] SubscriptionsForType;

        public readonly DateTime TimestampUtc;

        public PeerSubscriptionsForTypesUpdated(PeerId peerId, DateTime timestampUtc, MessageTypeId messageTypeId, params BindingKey[] bindingKeys)
        {
            PeerId = peerId;
            SubscriptionsForType = new[] { new SubscriptionsForType(messageTypeId, bindingKeys) };
            TimestampUtc = timestampUtc;
        }

        public PeerSubscriptionsForTypesUpdated(PeerId peerId, DateTime timestampUtc, params SubscriptionsForType[] subscriptionsForType)
        {
            PeerId = peerId;
            SubscriptionsForType = subscriptionsForType;
            TimestampUtc = timestampUtc;
        }

        public override string ToString() => $"PeerId: {PeerId}, TimestampUtc: {TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}";
    }
}
