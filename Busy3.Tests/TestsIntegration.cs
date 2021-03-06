﻿using Busy.Tests.Infrastructure;
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
            GlobalTestContext.Reset();

            var subscription1 = Subscription.Matching<DatabaseStatus>(status => status.DatacenterName == "Paris" && status.Status == "Ko");
            var subscription2 = Subscription.Any<DoSomething>();

            Assert.AreEqual(0, GlobalTestContext.Get());

            await _bus.Subscribe(new SubscriptionRequest(subscription1));

            await Task.Delay(50);

            await _bus.Subscribe(new SubscriptionRequest(subscription2));

            await Task.Delay(50);

            //should be consumed
            var command = new DoSomething();

            await _bus.Send(command);

            await Task.Delay(50);

            Assert.AreEqual(1, GlobalTestContext.Get());

            //should be consumed
            var @event = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"
            };

            _bus.Publish(@event);

            await Task.Delay(50);

            Assert.AreEqual(2, GlobalTestContext.Get());

            //should be consumed
            @event = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ko"
            };

            _bus.Publish(@event);

            await Task.Delay(50);

            Assert.AreEqual(3, GlobalTestContext.Get());

            //should not be consumed
            @event = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                Status = "Ok"
            };

            _bus.Publish(@event);

            await Task.Delay(50);

            Assert.AreEqual(3, GlobalTestContext.Get());

        }
    }
}
