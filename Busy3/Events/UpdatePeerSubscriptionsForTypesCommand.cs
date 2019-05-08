using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class UpdatePeerSubscriptionsForTypesCommand : ICommand
    {
        public PeerId PeerId { get; set; }

        public SubscriptionsForType[] SubscriptionsForTypes { get; set; }

        public DateTime TimestampUtc { get; set; }

        public UpdatePeerSubscriptionsForTypesCommand(PeerId peerId, DateTime timestampUtc, params SubscriptionsForType[] subscriptionsForTypes)
        {
            PeerId = peerId;
            SubscriptionsForTypes = subscriptionsForTypes;
            TimestampUtc = timestampUtc;
        }

        public UpdatePeerSubscriptionsForTypesCommand()
        {
        }

        public override string ToString() => $"{PeerId} TimestampUtc: {TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}";
    }
}
