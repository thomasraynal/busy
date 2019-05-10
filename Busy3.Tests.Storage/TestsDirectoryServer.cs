using Busy.Tests.Infrastructure;
using NUnit.Framework;
using StructureMap;
using System;
using System.Threading.Tasks;

namespace Busy.Tests.Storage
{
    [TestFixture]
    public class TestsDirectoryServer
    {
        private IBus _bus;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var container = new Container(configuration => configuration.AddRegistry<BusRegistry>());

            _bus = BusFactory.Create("TestE2E", "tcp://localhost:8080", "tcp://localhost:8080", container);
        }

        [SetUp]
        public void SetUp()
        {
            GlobalTestContext.Reset();
        }

        [Test]
        public async Task ShouldRegisterAndUnregisterPeer()
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

            await _bus.Subscribe(new SubscriptionRequest(subscription1));

            Assert.AreEqual(1, GlobalTestContext.Get());

            await _bus.Subscribe(new SubscriptionRequest(subscription2));

            Assert.AreEqual(1, GlobalTestContext.Get());

        }
    }
}
