﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    public class SubscriptionRequest
    {
        private readonly HashSet<Subscription> _subscriptions = new HashSet<Subscription>();

        public IEnumerable<Subscription> Subscriptions => _subscriptions;

        public bool ThereIsNoHandlerButIKnowWhatIAmDoing { get; set; }

        internal SubscriptionRequestBatch Batch { get; private set; }

        internal bool IsSubmitted { get; private set; }
        internal int? SubmissionSubscriptionsVersion { get; set; }

        public SubscriptionRequest(Subscription subscription)
        {
            _subscriptions.Add(subscription);
        }

        public SubscriptionRequest(IEnumerable<Subscription> subscriptions)
        {
            _subscriptions.AddRange(subscriptions);
        }

        public void AddToBatch(SubscriptionRequestBatch batch)
        {
            EnsureNotSubmitted();

            if (Batch != null)
                throw new InvalidOperationException("This subscription request is already part of a batch");

            batch.AddRequest(this);
            Batch = batch;
        }

        internal void MarkAsSubmitted()
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
