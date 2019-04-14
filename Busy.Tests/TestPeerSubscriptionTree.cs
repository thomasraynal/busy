using Busy.Infrastructure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Tests
{
    [TestFixture]
    public class TestPeerSubscriptionTree
    {
        [Test]
        public void ShouldBuildSubscriptionTree()
        {

            var peer1 = new Peer(new PeerId("Abc.Testing." + Guid.NewGuid()), "tcp://localhost:8080");
            var peer2 = new Peer(new PeerId("Abc.Testing." + Guid.NewGuid()), "tcp://localhost:8181");

            var messageKo = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"
     
            };

            var messageOk = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ok"
            };

            var koParis = BindingKey.Create(messageKo);
            var okParis = BindingKey.Create(messageOk);

            var allParisKo = Subscription.Matching<DatabaseStatus>(x => x.DatacenterName == "Paris" && x.Status == "Ko");

            var allParis = Subscription.Matching<DatabaseStatus>(x => x.DatacenterName == "Paris");

            var allKo = Subscription.Matching<DatabaseStatus>(x => x.Status == "Ko");

            var subscriptionTree = new PeerSubscriptionTree();

            subscriptionTree.Add(peer1, allParisKo.BindingKey);
            subscriptionTree.Add(peer2, allParis.BindingKey);


            var matchedPeers = subscriptionTree.GetPeers(allParis.BindingKey);
            matchedPeers = subscriptionTree.GetPeers(allKo.BindingKey);
            matchedPeers = subscriptionTree.GetPeers(allParisKo.BindingKey);
           
        }
    }
}
