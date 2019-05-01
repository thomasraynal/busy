using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public interface IPeer
    {
        PeerId Id { get; }
        string EndPoint { get; }
    }
}
