using Busy;
using Busy3.Demo.Shared;
using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;


namespace Busy3.PeerDirectory
{
    public class DemoDirectoryRegistry : Registry
    {
        public DemoDirectoryRegistry()
        {
            For<IPeerRepository>().Use<MemoryPeerRepository>().Singleton();
            For<IPeerDirectory>().Use<PeerDirectoryServer>().Singleton();
            Forward<IPeerDirectory, IPeerDirectoryServer>();
            Forward<IPeerDirectory, IMessageHandler<PeerStarted>>();
            Forward<IPeerDirectory, IMessageHandler<PeerStopped>>();
            Forward<IPeerDirectory, IMessageHandler<UpdatePeerSubscriptionsForTypesCommand>>();
            Forward<IPeerDirectory, IMessageHandler<PeerSubscriptionsForTypesUpdated>>();
        }

    }
}
