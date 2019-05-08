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

    [TestFixture]
    public class TestsIntegration
    {
        private IBus _bus;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _bus = BusFactory.Create("TestE2E", "tcp://localhost:8585", "tcp://localhost:8585");
            _bus.Start();
        }

        [Test]
        public async Task ShouldTestE2E()
        {
            TestsBusContext.Reset();

            var subscription1 = Subscription.Matching<DatabaseStatus>(status => status.DatacenterName == "Paris" && status.Status == "Ko");
            var subscription2 = Subscription.Any<DoSomething>();

            var command = new DoSomething();

            Assert.AreEqual(0, TestsBusContext.Get());

            var @event = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"
            };

            await _bus.Subscribe(new SubscriptionRequest(subscription1));

            await Task.Delay(500);

            await _bus.Subscribe(new SubscriptionRequest(subscription2));

            await Task.Delay(500);

            await _bus.Send(command);

            await Task.Delay(500);

            Assert.AreEqual(1, TestsBusContext.Get());

            _bus.Publish(@event);

            await Task.Delay(500);

            Assert.AreEqual(2, TestsBusContext.Get());

        }
    }
}
