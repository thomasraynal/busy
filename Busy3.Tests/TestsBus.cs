using Busy.Tests.Infrastructure;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busy.Tests
{
    public static class TestBusTransportTestContext
    {
        public static int TransportHitCounter { get; set; }
    }

    public class TestBusTransport : ITransport
    {
        public TestBusTransport(PeerId peerId)
        {
            PeerId = peerId;
        }

        public PeerId PeerId { get; }

        public string InboundEndPoint => throw new NotImplementedException();

        public event Action<TransportMessage> MessageReceived;

        public void AckMessage(TransportMessage transportMessage)
        {

        }

        public void Configure(PeerId peerId)
        {

        }

        public void Configure(PeerId peerId, string endpoint)
        {

        }

        public void Send(TransportMessage message, IEnumerable<Peer> peers)
        {
            TestBusTransportTestContext.TransportHitCounter++;
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }
    }

  
    [TestFixture]
    public class TestsBus
    {
        private IBus _bus;
        private IPeerDirectory _directory;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var container = new Container(configuration => configuration.AddRegistry<BusRegistry>());

            _directory = container.GetInstance<IPeerDirectory>();

            container.Configure((conf) =>
            {
                conf.For<ITransport>().Use<TestBusTransport>();
            });

            _bus = BusFactory.Create("TestE2E", "tcp://localhost:8080", "tcp://localhost:8080", container);
        }

        [SetUp]
        public void SetUp()
        {
            TestBusTransportTestContext.TransportHitCounter = 0;
        }

        [Test]
        public async Task ShouldMakeSubscription()
        {

            var subscription1 = Subscription.Matching<DatabaseStatus>(status => status.DatacenterName == "Paris" && status.Status == "Ko");
            var subscription2 = Subscription.Any<DoSomething>();

            var command = new DoSomething();

            Assert.AreEqual(0, GlobalTestContext.Get());

            var @event = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"
            };

            _bus.Start();

            //registration
            Assert.AreEqual(1, TestBusTransportTestContext.TransportHitCounter);

            await _bus.Subscribe(new SubscriptionRequest(subscription1));

            Assert.AreEqual(2, TestBusTransportTestContext.TransportHitCounter);

            await _bus.Subscribe(new SubscriptionRequest(subscription2));

            Assert.AreEqual(3, TestBusTransportTestContext.TransportHitCounter);

            //as the transport is mocked, register manually
            await _directory.RegisterAsync(_bus.Self, _bus.GetSubscriptions());

            //register directory transport call 
            Assert.AreEqual(4, TestBusTransportTestContext.TransportHitCounter);

            _bus.Publish(@event);

            Assert.AreEqual(5, TestBusTransportTestContext.TransportHitCounter);

            await _bus.Send(command);

            Assert.AreEqual(6, TestBusTransportTestContext.TransportHitCounter);

        }
    }
}
