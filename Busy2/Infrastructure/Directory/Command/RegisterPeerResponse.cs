using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public class RegisterPeerResponse : IMessage
    {
        public readonly PeerDescriptor[] PeerDescriptors;

        public RegisterPeerResponse(PeerDescriptor[] peerDescriptors)
        {
            PeerDescriptors = peerDescriptors;
        }
    }
}
