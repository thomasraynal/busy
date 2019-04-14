using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    [Transient]
    public class UpdatePeerSubscriptionsForTypesCommand : ICommand
    {
        public readonly PeerId PeerId;

        public readonly SubscriptionsForType[] SubscriptionsForTypes;

        public readonly DateTime TimestampUtc;

        public UpdatePeerSubscriptionsForTypesCommand(PeerId peerId, DateTime timestampUtc, params SubscriptionsForType[] subscriptionsForTypes)
        {
            PeerId = peerId;
            SubscriptionsForTypes = subscriptionsForTypes;
            TimestampUtc = timestampUtc;
        }

        public override string ToString() => $"{PeerId} TimestampUtc: {TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}";
    }
}
