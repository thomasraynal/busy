﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    internal class UniqueTimestampProvider
    {
        private DateTime _lastEventTimestamp = SystemDateTime.UtcNow;
        private readonly object _lastEventTimestampLock = new object();
        private int _incrementInTicks;

        public UniqueTimestampProvider(int incrementInTicks = 1)
        {
            _incrementInTicks = incrementInTicks;
        }

        public DateTime NextUtcTimestamp()
        {
            lock (_lastEventTimestampLock)
            {
                var currentTime = SystemDateTime.UtcNow;
                if (currentTime <= _lastEventTimestamp)
                    currentTime = _lastEventTimestamp.AddTicks(_incrementInTicks);
                _lastEventTimestamp = currentTime;
                return _lastEventTimestamp;
            }
        }
    }
}
