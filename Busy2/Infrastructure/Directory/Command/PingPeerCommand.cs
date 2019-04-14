using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    [Transient, Infrastructure]
    public sealed class PingPeerCommand : ICommand
    {
    }
}
