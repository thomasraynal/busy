using Busy.Infrastructure;
using NUnit.Framework;
using System;

namespace Busy.Tests
{
    [TestFixture]
    public class SubscriptionTests
    {
        [Test]
        public void ShouldParseCommand()
        {

            var id = Guid.NewGuid().ToString();

            var message = new DatabaseStatus()
            {
                DatabaseNodeId = id,
                DatacenterName = "Paris",
                FailureType = "Oh no!"
            };

            var key = BindingKey.Create(message);

            Assert.AreEqual($"{message.DatacenterName}.{message.DatabaseNodeId}.{message.FailureType}", key.ToString());

        }

        [Test]
        public void ShouldParseSubscription()
        {
            var subscription = Subscription.Matching<DatabaseStatus>(x => x.DatacenterName == "Paris");
            Assert.AreEqual($"DatabaseNodeFailureDetected (Paris.*.*)", subscription.ToString());
        }

        [Test]
        public void ShouldMatchSubscription()
        {
            var allowed = new DatabaseStatus()
            {
                DatabaseNodeId = Guid.NewGuid().ToString(),
                DatacenterName = "Paris",
                FailureType = "Oh no!"
            };

            var notAllowed = new DatabaseStatus()
            {
                DatabaseNodeId = Guid.NewGuid().ToString(),
                DatacenterName = "London",
                FailureType = "Oh no!"
            };

            var key = BindingKey.Create(allowed);

            var subscription = Subscription.Matching<DatabaseStatus>(x => x.DatacenterName == "Paris");

            var allowedMessageBinding = MessageBinding.FromMessage(allowed);
            var notAllowedMessageBinding = MessageBinding.FromMessage(notAllowed);

            Assert.IsTrue(subscription.Matches(allowedMessageBinding));

            Assert.IsFalse(subscription.Matches(notAllowedMessageBinding));
        }

    }
}
