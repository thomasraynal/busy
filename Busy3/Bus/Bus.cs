using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public class Bus : IBus
    {
        private readonly IPeerDirectory _directory;
        private readonly IMessageSerializer _serializer;
        private readonly ITransport _transport;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IBusConfiguration _configuration;
        private readonly Dictionary<Subscription, int> _subscriptions = new Dictionary<Subscription, int>();

        public Bus(IPeerDirectory directory, IMessageSerializer serializer, ITransport transport, IMessageDispatcher messageDispatcher, IBusConfiguration configuration)
        {
            _directory = directory;
            _messageDispatcher = messageDispatcher;
            _configuration = configuration;
            _serializer = serializer;
            _transport = transport;
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
        }

        public void Publish(IEvent message)
        {
            var peersHandlingMessage = _directory.GetPeersHandlingMessage(message);
            SendTransportMessage(null, message, peersHandlingMessage, true);
        }

        public Task<ICommandResult> Send(ICommand message)
        {
            var peers = _directory.GetPeersHandlingMessage(message);
            return Send(message, peers[0]);
        }

        public Task<ICommandResult> Send(ICommand message, Peer peer)
        {

            var transportMessage = ToTransportMessage(message);

            var peers = new[] { peer };

            _transport.Send(transportMessage, peers);

            var result = new CommandResult(0, string.Empty, null);

            return Task.FromResult(result as ICommandResult);

           

        }

        private void SendTransportMessage(Guid? messageId, IMessage message, IList<Peer> peers, bool logEnabled, bool locallyHandled = false)
        {
            var transportMessage = ToTransportMessage(message);

            if (messageId != null)
                transportMessage.Id = messageId.Value;

            _transport.Send(transportMessage, peers);
        }

        public void Start()
        {
            var self = new Peer(PeerId, EndPoint);
            _directory.RegisterAsync(this, self, GetSubscriptions()).Wait();
        }

        public void Stop()
        {
       
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

        protected TransportMessage ToTransportMessage(IMessage message) => _serializer.ToTransportMessage(message, PeerId, EndPoint);

        private IMessage ToMessage(TransportMessage transportMessage)
            => ToMessage(transportMessage.MessageTypeId, transportMessage.Content, transportMessage);

        private IMessage ToMessage(MessageTypeId messageTypeId, Stream messageStream, TransportMessage transportMessage)
        {
            return _serializer.ToMessage(transportMessage, messageTypeId, messageStream);
        }
    }
}
