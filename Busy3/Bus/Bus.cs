using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public class Bus : IBus
    {
        private readonly IPeerDirectory _directory;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IBusConfiguration _configuration;
        private readonly Dictionary<Subscription, int> _subscriptions = new Dictionary<Subscription, int>();

        public Bus(IPeerDirectory directory, IMessageDispatcher messageDispatcher, IBusConfiguration configuration)
        {
            _directory = directory;
            _messageDispatcher = messageDispatcher;
            _configuration = configuration;
        }

        public PeerId PeerId { get; private set; }

        public string EndPoint { get; private set; }

        public string DirectoryEndpoint => _configuration.DirectoryEndpoint;

        public void Configure(PeerId peerId, string endpoint)
        {
            PeerId = peerId;
            EndPoint = endpoint;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Publish(IEvent message)
        {
            var peers = _directory.GetPeersHandlingMessage(message);

            foreach(var peer in peers)
            {
                
            }
        }

        public Task<ICommandResult> Send(ICommand message)
        {
            throw new NotImplementedException();
        }

        public Task<ICommandResult> Send(ICommand message, Peer peer)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            var self = new Peer(PeerId, EndPoint);
            _directory.RegisterAsync(this, self, GetSubscriptions()).Wait();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public async Task Subscribe(SubscriptionRequest request)
        {

            request.MarkAsSubmitted();

            if (request.Batch != null)
                await request.Batch.WhenSubmittedAsync().ConfigureAwait(false);

            await AddSubscriptionsAsync(request).ConfigureAwait(false);

        }

        private async Task AddSubscriptionsAsync(SubscriptionRequest request)
        {

            if (request.Batch != null)
            {
                var batchSubscriptions = request.Batch.TryConsumeBatchSubscriptions();
                if (batchSubscriptions != null)
                {
                    try
                    {
                        await SendSubscriptionsAsync(batchSubscriptions).ConfigureAwait(false);
                        request.Batch.NotifyRegistrationCompleted(null);
                    }
                    catch (Exception ex)
                    {
                        request.Batch.NotifyRegistrationCompleted(ex);
                        throw;
                    }
                }
                else
                {
                    await request.Batch.WhenRegistrationCompletedAsync().ConfigureAwait(false);
                }
            }
            else
            {
                await SendSubscriptionsAsync(request.Subscriptions).ConfigureAwait(false);
            }
        }

        private async Task SendSubscriptionsAsync(IEnumerable<Subscription> subscriptions)
        {

            var updatedTypes = new HashSet<MessageTypeId>();

            lock (_subscriptions)
            {
                foreach (var subscription in subscriptions)
                {
                    var subscriptionCount = _subscriptions.GetValueOrDefault(subscription);
                    _subscriptions[subscription] = subscriptionCount + 1;

                    if (subscriptionCount <= 0)
                        updatedTypes.Add(subscription.MessageTypeId);
                }
            }

            if (updatedTypes.Count != 0)
            {
                await UpdateDirectorySubscriptionsAsync(updatedTypes).ConfigureAwait(false);
            }
        }

        private IEnumerable<Subscription> GetSubscriptions()
        {
            lock (_subscriptions)
            {
                return _subscriptions.Keys.ToList();
            }
        }

        private async Task UpdateDirectorySubscriptionsAsync(HashSet<MessageTypeId> updatedTypes)
        {
            var subscriptions = GetSubscriptions().Where(sub => updatedTypes.Contains(sub.MessageTypeId));
            var subscriptionsByTypes = SubscriptionsForType.CreateDictionary(subscriptions);

            var subscriptionUpdates = new List<SubscriptionsForType>(updatedTypes.Count);
            foreach (var updatedMessageId in updatedTypes)
                subscriptionUpdates.Add(subscriptionsByTypes.GetValueOrDefault(updatedMessageId, new SubscriptionsForType(updatedMessageId)));

            await _directory.UpdateSubscriptionsAsync(this, subscriptionUpdates).ConfigureAwait(false);
        }
    }
}
