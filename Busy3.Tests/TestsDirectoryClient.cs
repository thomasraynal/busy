using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Busy.Tests
{
    [TestFixture]
    public class TestsDirectoryClient
    {
        [Test]
        public void ShouldRetrieveClients()
        {
            var logger = new MockLogger();
            var directoryClient = new PeerDirectoryClient(logger);

            var messageParisKo = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"

            };

            var messageParisOk = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ok"
            };

            var messageLondonKo = new DatabaseStatus()
            {
                DatacenterName = "London",
                Status = "Ko"

            };

            var messageLondonOk = new DatabaseStatus()
            {
                DatacenterName = "London",
                Status = "Ok"
            };

            var peer1 = new Peer(new PeerId("Abc.Testing." + Guid.NewGuid()), "tcp://localhost:8080");
            var peer2 = new Peer(new PeerId("Abc.Testing." + Guid.NewGuid()), "tcp://localhost:8181");

            var allParisKo = Subscription.Matching<DatabaseStatus>(x => x.DatacenterName == "Paris" && x.Status == "Ko");
            var allParis = Subscription.Matching<DatabaseStatus>(x => x.DatacenterName == "Paris");
            var allKo = Subscription.Matching<DatabaseStatus>(x => x.Status == "Ko");

            var peer1Descriptor = new PeerDescriptor(peer1.Id, peer1.EndPoint, false, true, DateTime.Now, allParisKo, allParis);
            var peer2Descriptor = new PeerDescriptor(peer2.Id, peer2.EndPoint, false, true, DateTime.Now, allKo);

            directoryClient.Handle(new PeerStarted(peer1Descriptor));
            directoryClient.Handle(new PeerStarted(peer2Descriptor));

            var peers = directoryClient.GetPeersHandlingMessage(messageParisKo);

            Assert.AreEqual(2, peers.Count());

            peers = directoryClient.GetPeersHandlingMessage(messageParisOk);

            Assert.AreEqual(1, peers.Count());

            peers = directoryClient.GetPeersHandlingMessage(messageLondonKo);

            Assert.AreEqual(1, peers.Count());

            peers = directoryClient.GetPeersHandlingMessage(messageLondonOk);

            Assert.AreEqual(0, peers.Count());


        }
    }
}
