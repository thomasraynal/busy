using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    [Transient, Infrastructure]
    public sealed class PingPeerCommand : ICommand
    {
    }
}
