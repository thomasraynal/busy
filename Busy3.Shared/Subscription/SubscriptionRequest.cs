using System;
using System.Collections.Generic;
using System.Text;

namespace Busy
{
    public class SubscriptionRequest
    {
        private readonly HashSet<Subscription> _subscriptions = new HashSet<Subscription>();

        public IEnumerable<Subscription> Subscriptions => _subscriptions;

        public bool ThereIsNoHandlerButIKnowWhatIAmDoing { get; set; }

        public SubscriptionRequestBatch Batch { get; private set; }

        public bool IsSubmitted { get; private set; }

        public SubscriptionRequest(Subscription subscription)
        {
            _subscriptions.Add(subscription);
        }

        public SubscriptionRequest(IEnumerable<Subscription> subscriptions)
        {
            foreach(var subscription in subscriptions)
            {
                _subscriptions.Add(subscription);
            }
        }

        public void AddToBatch(SubscriptionRequestBatch batch)
        {
            EnsureNotSubmitted();

            if (Batch != null)
                throw new InvalidOperationException("This subscription request is already part of a batch");

            batch.AddRequest(this);
            Batch = batch;
        }

        public void MarkAsSubmitted()
        {
            EnsureNotSubmitted();
            IsSubmitted = true;
        }

        private void EnsureNotSubmitted()
        {
            if (IsSubmitted)
                throw new InvalidOperationException("This subscription request has already been submitted");
        }
    }
}
