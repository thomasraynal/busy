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
    public class DosSomethingCommandHandler : IMessageHandler<DoSomething>
    {
        public void Handle(DoSomething message)
        {
            TestsBusContext.Increment();
        }

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

        public void Send(TransportMessage message, IEnumerable<Peer> peers)
        {
            TestsBusContext.Increment();
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }
    }

    public static class TestsBusContext
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

    [TestFixture]
    public class TestsBus
    {
        private Bus _bus;
        private PeerId _peerId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            
            var logger = new MockLogger();
            var directoryClient = new PeerDirectoryClient();
            var container = new Container(configuration => configuration.AddRegistry<AppRegistry>());
            var messageDispatcher = new MessageDispatcher(logger, container);
            var busConfiguration = new BusConfiguration("http://localhost:8080");
            var serializer = new JsonMessageSerializer();

            _peerId = new PeerId($"{Guid.NewGuid()}");

            var transport = new TestBusTransport(_peerId);
            _bus = new Bus(directoryClient, serializer, transport, messageDispatcher, busConfiguration);

        }

        [SetUp]
        public void SetUp()
        {
            TestsBusContext.Reset();
        }

        [Test]
        public async Task ShouldMakeSubscription()
        {

            var subscription1 = Subscription.Matching<DatabaseStatus>(status => status.DatacenterName == "Paris" && status.Status == "Ko");
            var subscription2 = Subscription.Any<DoSomething>();


            var command = new DoSomething();

            Assert.AreEqual(0, TestsBusContext.Get());

            var @event = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"
            };

            _bus.Configure(_peerId, "http://localhost:8080");

            _bus.Start();

            await _bus.Subscribe(new SubscriptionRequest(subscription1));
            await _bus.Subscribe(new SubscriptionRequest(subscription2));

            _bus.Publish(@event);

            Assert.AreEqual(1, TestsBusContext.Get());

            await _bus.Send(command);

            Assert.AreEqual(2, TestsBusContext.Get());

        }
    }
}
