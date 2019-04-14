using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public interface IDeadPeerDetector : IDisposable
    {
        event Action<Exception> Error;
        event Action PersistenceDownDetected;
        event Action<PeerId, DateTime> PingTimeout;

        void Start();
        void Stop();
    }
}
