using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public class DirectoryConfiguration : IDirectoryConfiguration
    {
        public DirectoryConfiguration()
        {
            PeerPingInterval = 1.Minute();
            TransientPeerPingTimeout = 5.Minutes();
            PersistentPeerPingTimeout =5.Minutes();
            DebugPeerPingTimeout = 10.Minutes();
            BlacklistedMachines = new string[0];
            DisableDynamicSubscriptionsForDirectoryOutgoingMessages = false;
            WildcardsForPeersNotToDecommissionOnTimeout = new string[0];
            MaxAllowedClockDifferenceWhenRegistering = null;
        }


        public TimeSpan PeerPingInterval { get; private set; }
        public TimeSpan TransientPeerPingTimeout { get; private set; }
        public TimeSpan PersistentPeerPingTimeout { get; private set; }
        public TimeSpan DebugPeerPingTimeout { get; private set; }
        public string[] BlacklistedMachines { get; private set; }
        public string[] WildcardsForPeersNotToDecommissionOnTimeout { get; private set; }
        public bool DisableDynamicSubscriptionsForDirectoryOutgoingMessages { get; private set; }
        public TimeSpan? MaxAllowedClockDifferenceWhenRegistering { get; }
    }
}
