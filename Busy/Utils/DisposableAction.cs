using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Busy
{
    [Serializable]
    internal sealed class DisposableAction : IDisposable
    {
        private Action _action;

        public DisposableAction(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _action, null)?.Invoke();
        }
    }
}
