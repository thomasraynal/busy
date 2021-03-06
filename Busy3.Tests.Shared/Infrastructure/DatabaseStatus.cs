﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    [Routable]
    [Asynchronous]
    public class DatabaseStatus : IEvent
    {
        [RoutingPosition(1)]
        public string DatacenterName;

        [RoutingPosition(2)]
        public string Status = "Ko";

        public string FailureType;
    }


    public class DatabaseNodeFailureHandler : IMessageHandler<DatabaseStatus>
    {
        public static bool HasBeenHandled { get; private set; }

        public void Handle(DatabaseStatus message)
        {
            HasBeenHandled = true;
        }
    }
}
