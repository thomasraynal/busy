using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class PeerSubscriptionsForTypesUpdated : IEvent
    {
        public PeerId PeerId { get; set; }

        public SubscriptionsForType[] SubscriptionsForType { get; set; }

        public DateTime TimestampUtc { get; set; }

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

        public PeerSubscriptionsForTypesUpdated(PeerId peerId, SubscriptionsForType[] subscriptionsForType, DateTime timestampUtc)
        {
            PeerId = peerId;
            SubscriptionsForType = subscriptionsForType;
            TimestampUtc = timestampUtc;
        }

        public PeerSubscriptionsForTypesUpdated()
        {
        }

        public override string ToString() => $"PeerId: {PeerId}, TimestampUtc: {TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}";
    }
}
