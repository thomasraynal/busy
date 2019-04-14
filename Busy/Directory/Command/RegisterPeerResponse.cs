using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
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
