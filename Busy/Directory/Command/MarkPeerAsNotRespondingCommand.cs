using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class MarkPeerAsNotRespondingCommand : ICommand
    {
        public readonly PeerId PeerId;

        public readonly DateTime TimestampUtc;

        public MarkPeerAsNotRespondingCommand(PeerId peerId, DateTime timestampUtc)
        {
            PeerId = peerId;
            TimestampUtc = timestampUtc;
        }

        public override string ToString() => PeerId.ToString();
    }
}
