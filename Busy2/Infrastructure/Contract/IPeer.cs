using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface IPeer
    {
        PeerId Id { get; }
        string EndPoint { get; }
    }
}
