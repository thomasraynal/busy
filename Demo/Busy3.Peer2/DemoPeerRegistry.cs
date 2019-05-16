using Busy;
using Busy3.Demo.Shared;
using Microsoft.Extensions.Logging;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;


namespace Busy3.Peer2
{
    public class DemoPeerRegistry : Registry
    {
        public DemoPeerRegistry()
        {
            For<IPeerDirectoryClient>().Use<PeerDirectoryClient>().Singleton();
            Forward<IPeerDirectoryClient, IPeerDirectory>();
            Forward<IPeerDirectory, IMessageHandler<PeerActivated>>();
            Forward<IPeerDirectory, IMessageHandler<PeerStopped>>();
            Forward<IPeerDirectory, IMessageHandler<UpdatePeerSubscriptionsForTypesCommand>>();
            Forward<IPeerDirectory, IMessageHandler<PeerSubscriptionsForTypesUpdated>>();
        }
    }
}
