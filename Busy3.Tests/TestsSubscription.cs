using NUnit.Framework;
using System;

namespace Busy.Tests
{
    [TestFixture]
    public class TestsSubscription
    {
        [Test]
        public void ShouldParseCommand()
        {

            var message = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                FailureType = "Oh no!"
            };

            var key = BindingKey.Create(message);

            Assert.AreEqual($"{message.DatacenterName}.{message.Status}", key.ToString());

        }

        [Test]
        public void ShouldParseSubscription()
        {
            var subscription = Subscription.Matching<DatabaseStatus>(x => x.DatacenterName == "Paris");
            Assert.AreEqual($"DatabaseStatus (Paris.*)", subscription.ToString());
        }

        [Test]
        public void ShouldMatchSubscription()
        {
            var allowed = new DatabaseStatus()
            {
                DatacenterName = "Paris",
                FailureType = "Oh no!"
            };

            var notAllowed = new DatabaseStatus()
            {
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
