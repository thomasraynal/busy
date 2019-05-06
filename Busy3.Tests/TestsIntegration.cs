using Busy.Tests.Infrastructure;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy.Tests
{
    public class DoSomethingCommandHandler : IMessageHandler<DoSomething>
    {
        public void Handle(DoSomething message)
        {
            TestsBusContext.Increment();
        }

    }


    public class DatabaseStatusEventHandler : IMessageHandler<DatabaseStatus>
    {
        public void Handle(DatabaseStatus message)
        {
            TestsBusContext.Increment();
        }

    }


    [TestFixture]
    public class TestsIntegration
    {
        private Bus _bus;
        private TransportConfiguration _transportConfiguration;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {

            var logger = new MockLogger();
            var directoryClient = new PeerDirectoryClient();
            var container = new Container(configuration => configuration.AddRegistry<AppRegistry>());
            var messageDispatcher = new MessageDispatcher(logger, container);

            _transportConfiguration = new TransportConfiguration()
            {
                InboundEndPoint = "tcp://localhost:8080",
                PeerId = new PeerId($"{Guid.NewGuid()}")
            };



            var busConfiguration = new BusConfiguration(_transportConfiguration.InboundEndPoint);
            var serializer = new JsonMessageSerializer();


            var transport = new Transport(_transportConfiguration, serializer, logger);

            _bus = new Bus(directoryClient, serializer, transport, messageDispatcher, busConfiguration);

          
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

            _bus.Configure(_transportConfiguration.PeerId, _transportConfiguration.InboundEndPoint);

            _bus.Start();

            await _bus.Subscribe(new SubscriptionRequest(subscription1));
            await _bus.Subscribe(new SubscriptionRequest(subscription2));

            //_bus.Publish(@event);

            //await Task.Delay(200);

            //Assert.AreEqual(1, TestsBusContext.Get());

            await _bus.Send(command);

            await Task.Delay(200);

            Assert.AreEqual(1, TestsBusContext.Get());

        }
    }
}
