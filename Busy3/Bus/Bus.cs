﻿using StructureMap;
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
        private ITransport _transport;
        private readonly IContainer _container;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly Dictionary<Subscription, int> _subscriptions = new Dictionary<Subscription, int>();
       
        public Bus(IPeerDirectory directory, IMessageSerializer serializer, IMessageDispatcher messageDispatcher, IContainer container)
        {
            _directory = directory;
            _messageDispatcher = messageDispatcher;
            _serializer = serializer;
            _container = container;
        }

        public IEnumerable<Subscription> AutoSubscribes { get; } = new[]{
                    Subscription.Any<UpdatePeerSubscriptionsForTypesCommand>(),
                    Subscription.Any<PeerStopped>(),
                    Subscription.Any<PeerActivated>(),
                    Subscription.Any<PeerStarted>(),
                    Subscription.Any<PeerSubscriptionsForTypesUpdated>()
                };

        public PeerId PeerId { get; internal set; }

        public Peer Self { get; internal set; }

        public string EndPoint { get; internal set; }

        public IContainer Container => _container;

        public string DirectoryEndpoint { get; internal set; }

        public void Configure(PeerId peerId, string endpoint, string directoryEndpoint)
        {
            PeerId = peerId;
            EndPoint = endpoint;
            DirectoryEndpoint = directoryEndpoint;
        }

        public void Dispose()
        {
        }

        public void Publish(IEvent message)
        {
            var peersHandlingMessage = _directory.GetPeersHandlingMessage(message);

            if (peersHandlingMessage.Count == 0) return;

            SendTransportMessage(null, message, peersHandlingMessage);
        }

        public async Task<ICommandResult> Send(ICommand message)
        {
            var peers = _directory.GetPeersHandlingMessage(message);

            if (peers.Count == 0) return CommandResult.Empty;

            return await Send(message, peers[0]);
        }

        public Task<ICommandResult> Send(ICommand message, Peer peer)
        {

            var transportMessage = ToTransportMessage(message);

            var peers = new[] { peer };

            _transport.Send(transportMessage, peers);

            var result = new CommandResult(0, string.Empty, null);

            return Task.FromResult(result as ICommandResult);

        }

        private void SendTransportMessage(Guid? messageId, IMessage message, IList<Peer> peers)
        {
            var transportMessage = ToTransportMessage(message);

            if (messageId != null)
                transportMessage.Id = messageId.Value;

            _transport.Send(transportMessage, peers);
        }

        public void Start()
        {
            Self = new Peer(PeerId, EndPoint);

            _directory.Configure(this);

            _transport = _container.GetInstance<ITransport>();
            _transport.Configure(PeerId, EndPoint);

            _transport.Start();

            _transport.MessageReceived += (transportMessage) =>
            {
                var message = ToMessage(transportMessage);
                var dispach = new MessageDispatch(message);
                _messageDispatcher.Dispatch(dispach);
            };

            PerformAutoSubscribe();
           
            Task.Delay(1000).Wait();

        }

        public void Stop()
        {
            _transport.Stop();
            _subscriptions.Clear();
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

        private void PerformAutoSubscribe()
        {
            _directory.RegisterAsync(Self, AutoSubscribes).Wait();
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

        public IEnumerable<Subscription> GetSubscriptions()
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

            await _directory.UpdateSubscriptionsAsync(subscriptionUpdates).ConfigureAwait(false);
        }

        protected TransportMessage ToTransportMessage(IMessage message) => _serializer.ToTransportMessage(message, PeerId, EndPoint);

        private IMessage ToMessage(TransportMessage transportMessage)
            => ToMessage(transportMessage.MessageTypeId, transportMessage.Content, transportMessage);

        private IMessage ToMessage(MessageTypeId messageTypeId, byte[] messageStream, TransportMessage transportMessage)
        {
            return _serializer.ToMessage(transportMessage);
        }
    }
}
