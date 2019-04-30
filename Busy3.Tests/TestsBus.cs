using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busy.Tests
{
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
        [Test]
        public async Task ShouldMakeSubscription()
        {
            var logger = new MockLogger();
            var directoryClient = new PeerDirectoryClient();
            var container = new Container(configuration => configuration.AddRegistry<AppRegistry>());
            var messageDispatcher = new MessageDispatcher(logger, container);
            var busConfiguration = new BusConfiguration("http://localhost:8080");
            var bus = new Bus(directoryClient, messageDispatcher, busConfiguration);

            var subscription = Subscription.Matching<DatabaseStatus>(status => status.DatacenterName == "Paris" && status.Status == "Ko");

            Assert.AreEqual(0, TestsBusContext.Get());

         
            var @event = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"
            };

            var peerId = new PeerId("self".Replace("*", Guid.NewGuid().ToString()));

            bus.Configure(peerId, "http://localhost:8080");

            bus.Start();

            await bus.Subscribe(new SubscriptionRequest(subscription));


            bus.Publish(@event);

         //   Assert.AreEqual(1, TestsBusContext.Get());

        }
    }
}
