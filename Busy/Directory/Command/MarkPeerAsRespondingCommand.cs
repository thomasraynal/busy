using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class MarkPeerAsRespondingCommand : ICommand
    {
        public readonly PeerId PeerId;

        public readonly DateTime TimestampUtc;

        public MarkPeerAsRespondingCommand(PeerId peerId, DateTime timestampUtc)
        {
            PeerId = peerId;
            TimestampUtc = timestampUtc;
        }

        public override string ToString() => PeerId.ToString();
    }
}
