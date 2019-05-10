using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Busy.Tests
{
    public static class GlobalTestContext
    {
        private static volatile int _counter;

        public static int Get()
        {
            return _counter;
        }

        public static void Increment()
        {
            Interlocked.Increment(ref _counter);
        }

        public static void Reset()
        {
            Interlocked.Exchange(ref _counter, 0);
        }
    }

}
