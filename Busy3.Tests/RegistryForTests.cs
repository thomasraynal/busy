﻿using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    public class TestRegistry : Registry
    {
        public TestRegistry()
        {
            For<ILogger>().Use<MockLogger>().Singleton();
            For<IMessageSerializer>().Use<JsonMessageSerializer>();
            For<IPeerDirectoryClient>().Use<PeerDirectoryClient>().Singleton();
            Forward<IPeerDirectoryClient, IPeerDirectory>();
            Forward<IPeerDirectory, IMessageHandler<PeerActivated>>();
            Forward<IPeerDirectory, IMessageHandler<PeerStopped>>();
            Forward<IPeerDirectory, IMessageHandler<UpdatePeerSubscriptionsForTypesCommand>>();
            Forward<IPeerDirectory, IMessageHandler<PeerSubscriptionsForTypesUpdated>>();
        }
    }
}
