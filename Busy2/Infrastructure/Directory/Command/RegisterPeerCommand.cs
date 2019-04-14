using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    [Transient]
    public sealed class RegisterPeerCommand : ICommand
    {
        public readonly PeerDescriptor Peer;

        public RegisterPeerCommand(PeerDescriptor peer)
        {
            Peer = peer;
        }

        public override string ToString() => $"{Peer.Peer} TimestampUtc: {Peer.TimestampUtc:yyyy-MM-dd HH:mm:ss.fff}";
    }
}
