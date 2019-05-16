using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public interface IPeerDirectoryServer : IPeerDirectory, IMessageHandler<PeerStarted>
    {
    }
}
