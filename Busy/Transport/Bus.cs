using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Busy
{
    public class Bus : IBus
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(Bus));

        private readonly ConcurrentDictionary<MessageId, TaskCompletionSource<CommandResult>> _messageIdToTaskCompletionSources = new ConcurrentDictionary<MessageId, TaskCompletionSource<CommandResult>>();
        private readonly UniqueTimestampProvider _deserializationFailureTimestampProvider = new UniqueTimestampProvider();
        private readonly Dictionary<Subscription, int> _subscriptions = new Dictionary<Subscription, int>();
        private readonly HashSet<MessageTypeId> _pendingUnsubscriptions = new HashSet<MessageTypeId>();
        private readonly ITransport _transport;
        private readonly IPeerDirectory _directory;
        private readonly IMessageSerializer _serializer;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IBusConfiguration _busConfiguration;
        private Task _processPendingUnsubscriptionsTask;

        private int _subscriptionsVersion;
        private int _status;

        public Bus(IBusConfiguration busConfiguration, ITransport transport, IPeerDirectory directory, IMessageSerializer serializer, IMessageDispatcher messageDispatcher)
        {
            _transport = transport;
            _transport.MessageReceived += OnTransportMessageReceived;
            _directory = directory;
            _serializer = serializer;
            _directory.PeerUpdated += OnPeerUpdated;
            _messageDispatcher = messageDispatcher;
            _busConfiguration = busConfiguration;
        }

        public PeerId PeerId { get; private set; }
        public bool IsRunning => Status == BusStatus.Started || Status == BusStatus.Stopping;
        public string EndPoint => _transport.InboundEndPoint;
        public string DirectoryEndpoint => _busConfiguration.DirectoryEndpoint;

        private BusStatus Status
        {
            get => (BusStatus)_status;
            set => _status = (int)value;
        }

        public void Configure(PeerId peerId)
        {
            PeerId = peerId;
            _transport.Configure(peerId);
        }

        public virtual void Start()
        {
            if (Interlocked.CompareExchange(ref _status, (int)BusStatus.Starting, (int)BusStatus.Stopped) != (int)BusStatus.Stopped)
                throw new InvalidOperationException("Unable to start, the bus is already running");


            var registered = false;

            try
            {
                _logger.DebugFormat("Loading invokers...");
                _messageDispatcher.LoadMessageHandlerInvokers();

                PerformAutoSubscribe();

                _logger.DebugFormat("Starting message dispatcher...");
                _messageDispatcher.Start();

                _logger.DebugFormat("Starting transport...");
                _transport.Start();

                Status = BusStatus.Started;

                _logger.DebugFormat("Registering on directory...");
                var self = new Peer(PeerId, EndPoint);
                _directory.RegisterAsync(this, self, GetSubscriptions()).Wait();
                registered = true;

                _transport.OnRegistered();
            }
            catch
            {
                InternalStop(registered);
                Status = BusStatus.Stopped;
                throw;
            }

        }

        private void PerformAutoSubscribe()
        {
            _logger.DebugFormat("Performing auto subscribe...");

            var autoSubscribeInvokers = _messageDispatcher.GetMessageHanlerInvokers().Where(x => x.ShouldBeSubscribedOnStartup).ToList();

            lock (_subscriptions)
            {
                foreach (var invoker in autoSubscribeInvokers)
                {
                    var subscription = new Subscription(invoker.MessageTypeId);
                    _subscriptions[subscription] = 1 + _subscriptions.GetValueOrDefault(subscription);
                }
            }
        }

        protected virtual IEnumerable<Subscription> GetSubscriptions()
        {
            lock (_subscriptions)
            {
                return _subscriptions.Keys.ToList();
            }
        }

        public virtual void Stop()
        {
            if (Interlocked.CompareExchange(ref _status, (int)BusStatus.Stopping, (int)BusStatus.Started) != (int)BusStatus.Started)
                throw new InvalidOperationException("Unable to stop, the bus is not running");

            InternalStop(true);

        }

        private void InternalStop(bool unregister)
        {
            Status = BusStatus.Stopping;

            if (unregister)
            {
                try
                {
                    _directory.UnregisterAsync(this).Wait();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            lock (_subscriptions)
            {
                _subscriptions.Clear();
                _pendingUnsubscriptions.Clear();
                _processPendingUnsubscriptionsTask = null;

                unchecked
                {
                    ++_subscriptionsVersion;
                }
            }

            try
            {
               
            }
            finally
            {
                Status = BusStatus.Stopped;
            }

            _messageIdToTaskCompletionSources.Clear();
        }

        public void Publish(IEvent message)
        {
            if (!IsRunning)
                throw new InvalidOperationException("Unable to publish message, the bus is not running");

            var peersHandlingMessage = _directory.GetPeersHandlingMessage(message);

            SendTransportMessage(null, message, peersHandlingMessage, true, false);
        }

        public Task<CommandResult> Send(ICommand message)
        {
            if (!IsRunning)
                throw new InvalidOperationException("Unable to send message, the bus is not running");

            var peers = _directory.GetPeersHandlingMessage(message);
            if (peers.Count == 0)
                throw new InvalidOperationException("Unable to find peer for specified command, " + message + ". Did you change the namespace?");

            var self = peers.FirstOrDefault(x => x.Id == PeerId);

            if (self != null)
                return Send(message, self);

            if (peers.Count > 1)
            {
                var exceptionMessage = $"{peers.Count} peers are handling {message}. Peers: {string.Join(", ", peers.Select(p => p.ToString()))}.";
                throw new InvalidOperationException(exceptionMessage);
            }

            return Send(message, peers[0]);
        }

        public Task<CommandResult> Send(ICommand message, Peer peer)
        {
            if (peer == null)
                throw new ArgumentNullException(nameof(peer));

            if (!IsRunning)
                throw new InvalidOperationException("Unable to send message, the bus is not running");

            var taskCompletionSource = new TaskCompletionSource<CommandResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (peer.Id == PeerId)
            {
                HandleLocalMessage(message, taskCompletionSource);
            }
            else
            {
                var transportMessage = ToTransportMessage(message);

                if (!peer.IsResponding && !message.TypeId().IsInfrastructure())
                    throw new InvalidOperationException($"Unable to send this transient message {message} while peer {peer.Id} is not responding.");

                _messageIdToTaskCompletionSources.TryAdd(transportMessage.Id, taskCompletionSource);

                var peers = new[] { peer };
                LogMessageSend(message, transportMessage, peers);
                SendTransportMessage(transportMessage, peers);
            }

            return taskCompletionSource.Task;
        }

        public async Task<IDisposable> SubscribeAsync(SubscriptionRequest request)
        {
            ValidateSubscriptionRequest(request);

            if (!request.ThereIsNoHandlerButIKnowWhatIAmDoing)
                EnsureMessageHandlerInvokerExists(request.Subscriptions);

            request.MarkAsSubmitted();

            if (request.Batch != null)
                await request.Batch.WhenSubmittedAsync().ConfigureAwait(false);

            await AddSubscriptionsAsync(request).ConfigureAwait(false);

            return new DisposableAction(() => RemoveSubscriptions(request));
        }

        public async Task<IDisposable> SubscribeAsync(SubscriptionRequest request, Action<IMessage> handler)
        {
            ValidateSubscriptionRequest(request);

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            request.MarkAsSubmitted();

            var eventHandlerInvokers = request.Subscriptions
                                              .GroupBy(x => x.MessageTypeId)
                                              .Select(x => new DynamicMessageHandlerInvoker(handler, x.Key.GetMessageType(), x.Select(s => s.BindingKey).ToList()))
                                              .ToList();

            if (request.Batch != null)
                await request.Batch.WhenSubmittedAsync().ConfigureAwait(false);

            foreach (var eventHandlerInvoker in eventHandlerInvokers)
                _messageDispatcher.AddInvoker(eventHandlerInvoker);

            await AddSubscriptionsAsync(request).ConfigureAwait(false);

            return new DisposableAction(() =>
            {
                foreach (var eventHandlerInvoker in eventHandlerInvokers)
                    _messageDispatcher.RemoveInvoker(eventHandlerInvoker);

                RemoveSubscriptions(request);
            });
        }

        private void ValidateSubscriptionRequest(SubscriptionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!IsRunning)
                throw new InvalidOperationException("Cannot perform a subscription, the bus is not running");
        }

        private void EnsureMessageHandlerInvokerExists(IEnumerable<Subscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                if (_messageDispatcher.GetMessageHanlerInvokers().All(x => x.MessageTypeId != subscription.MessageTypeId))
                    throw new ArgumentException($"No handler available for message type Id: {subscription.MessageTypeId}");
            }
        }

        private async Task AddSubscriptionsAsync(SubscriptionRequest request)
        {
            request.SubmissionSubscriptionsVersion = _subscriptionsVersion;

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

            async Task SendSubscriptionsAsync(IEnumerable<Subscription> subscriptions)
            {
                if (!IsRunning)
                    throw new InvalidOperationException("Cannot perform a subscription, the bus is not running");

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
                    // Wait until all unsubscriptions are completed to prevent race conditions
                    await WhenUnsubscribeCompletedAsync().ConfigureAwait(false);
                    await UpdateDirectorySubscriptionsAsync(updatedTypes).ConfigureAwait(false);
                }
            }
        }

        internal async Task WhenUnsubscribeCompletedAsync()
        {
            Task task;

            lock (_subscriptions)
            {
                task = _processPendingUnsubscriptionsTask;
            }

            if (task == null)
                return;

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
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


        private void RemoveSubscriptions(SubscriptionRequest request)
        {
            if (!IsRunning)
                return;

            lock (_subscriptions)
            {
                if (request.SubmissionSubscriptionsVersion != _subscriptionsVersion)
                    return;

                foreach (var subscription in request.Subscriptions)
                {
                    var subscriptionCount = _subscriptions.GetValueOrDefault(subscription);
                    if (subscriptionCount <= 1)
                    {
                        _subscriptions.Remove(subscription);
                        _pendingUnsubscriptions.Add(subscription.MessageTypeId);
                    }
                    else
                    {
                        _subscriptions[subscription] = subscriptionCount - 1;
                    }
                }

                if (_pendingUnsubscriptions.Count != 0 && _processPendingUnsubscriptionsTask?.IsCompleted != false)
                {
                    var subscriptionsVersion = _subscriptionsVersion;
                    _processPendingUnsubscriptionsTask = Task.Run(() => ProcessPendingUnsubscriptions(subscriptionsVersion));
                }
            }
        }

        private async Task ProcessPendingUnsubscriptions(int subscriptionsVersion)
        {
            try
            {
                var updatedTypes = new HashSet<MessageTypeId>();

                while (true)
                {
                    updatedTypes.Clear();

                    lock (_subscriptions)
                    {
                        updatedTypes.UnionWith(_pendingUnsubscriptions);
                        _pendingUnsubscriptions.Clear();

                        if (updatedTypes.Count == 0 || !IsRunning || Status == BusStatus.Stopping || subscriptionsVersion != _subscriptionsVersion)
                        {
                            _processPendingUnsubscriptionsTask = null;
                            return;
                        }
                    }

                    await UpdateDirectorySubscriptionsAsync(updatedTypes).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                lock (_subscriptions)
                {
                    _processPendingUnsubscriptionsTask = null;
                }
            }
        }

        private void OnPeerUpdated(PeerId peerId, PeerUpdateAction peerUpdateAction)
            => _transport.OnPeerUpdated(peerId, peerUpdateAction);

        private void OnTransportMessageReceived(TransportMessage transportMessage)
        {
            if (transportMessage.MessageTypeId == MessageExecutionCompleted.TypeId)
            {
                HandleMessageExecutionCompleted(transportMessage);
            }
            else
            {
                var executeSynchronously = transportMessage.MessageTypeId.IsInfrastructure();
                HandleRemoteMessage(transportMessage, executeSynchronously);
            }
        }

        public MessageDispatch CreateMessageDispatch(TransportMessage transportMessage)
            => CreateMessageDispatch(transportMessage, synchronousDispatch: false, sendAcknowledgment: false);

        private MessageDispatch CreateMessageDispatch(TransportMessage transportMessage, bool synchronousDispatch, bool sendAcknowledgment = true)
        {
            var message = ToMessage(transportMessage);
            if (message == null)
                return null;

            var context = MessageContext.CreateNew(transportMessage);
            var continuation = GetOnRemoteMessageDispatchedContinuation(transportMessage, sendAcknowledgment);
            return new MessageDispatch(context, message, continuation, synchronousDispatch);
        }

        protected virtual void HandleRemoteMessage(TransportMessage transportMessage, bool synchronous = false)
        {
            var dispatch = CreateMessageDispatch(transportMessage, synchronous);
            if (dispatch == null)
            {
                _logger.WarnFormat("Received a remote message that could not be deserialized: {0} from {1}", transportMessage.MessageTypeId.FullName, transportMessage.Originator.SenderId);
                _transport.AckMessage(transportMessage);
                return;
            }

            _logger.DebugFormat("RECV remote: {0} from {3} ({2} bytes). [{1}]", dispatch.Message, transportMessage.Id, transportMessage.Content.Length, transportMessage.Originator.SenderId);
            _messageDispatcher.Dispatch(dispatch);
        }

        private Action<MessageDispatch, DispatchResult> GetOnRemoteMessageDispatchedContinuation(TransportMessage transportMessage, bool sendAcknowledgment)
        {
            return (dispatch, dispatchResult) =>
            {
                HandleDispatchErrors(dispatch, dispatchResult, transportMessage);

                if (!sendAcknowledgment)
                    return;

                if (dispatch.Message is ICommand)
                {
                    var messageExecutionCompleted = MessageExecutionCompleted.Create(dispatch.Context, dispatchResult, _serializer);
                    SendTransportMessage(null, messageExecutionCompleted, dispatch.Context.GetSender(), false);
                }

                AckTransportMessage(transportMessage);
            };
        }

        private void HandleDispatchErrors(MessageDispatch dispatch, DispatchResult dispatchResult, TransportMessage failingTransportMessage = null)
        {
            if (!IsRunning || dispatchResult.Errors.Count == 0 || dispatchResult.Errors.All(error => error is DomainException))
                return;

            var errorMessages = dispatchResult.Errors.Select(error => error.ToString());
            var errorMessage = string.Join(System.Environment.NewLine + System.Environment.NewLine, errorMessages);

            try
            {
                if (failingTransportMessage == null)
                    failingTransportMessage = ToTransportMessage(dispatch.Message);
            }
            catch (Exception ex)
            {
                HandleDispatchErrorsForUnserializableMessage(dispatch.Message, ex, errorMessage);
                return;
            }

            string jsonMessage;
            try
            {
                jsonMessage = JsonConvert.SerializeObject(dispatch.Message);
            }
            catch (Exception ex)
            {
                jsonMessage = $"Unable to serialize message :{System.Environment.NewLine}{ex}";
            }

            var messageProcessingFailed = new MessageProcessingFailed(failingTransportMessage, jsonMessage, errorMessage, SystemDateTime.UtcNow, dispatchResult.ErrorHandlerTypes.Select(x => x.FullName).ToArray());
            Publish(messageProcessingFailed);
        }

        private void HandleDispatchErrorsForUnserializableMessage(IMessage message, Exception serializationException, string dispatchErrorMessage)
        {
            var messageTypeName = message.GetType().FullName;
            _logger.Error($"Unable to serialize message {messageTypeName}. Error: {serializationException}");

            if (!IsRunning)
                return;

            var errorMessage = $"Unable to handle local message\r\nMessage is not serializable\r\nMessageType: {messageTypeName}\r\nDispatch error: {dispatchErrorMessage}\r\nSerialization error: {serializationException}";
            var processingFailed = new CustomProcessingFailed(GetType().FullName, errorMessage, SystemDateTime.UtcNow);
            Publish(processingFailed);
        }

        private void HandleMessageExecutionCompleted(TransportMessage transportMessage)
        {
            var message = (MessageExecutionCompleted)ToMessage(transportMessage);
            if (message == null)
                return;

            HandleMessageExecutionCompleted(transportMessage, message);
        }

        protected virtual void HandleMessageExecutionCompleted(TransportMessage transportMessage, MessageExecutionCompleted message)
        {
            _logger.DebugFormat("RECV: {0}", message);

            if (!_messageIdToTaskCompletionSources.TryRemove(message.SourceCommandId, out var taskCompletionSource))
                return;

            var response = message.PayloadTypeId != null ? ToMessage(message.PayloadTypeId.Value, new MemoryStream(message.Payload), transportMessage) : null;
            var commandResult = new CommandResult(message.ErrorCode, message.ResponseMessage, response);

            taskCompletionSource.SetResult(commandResult);
        }

        protected virtual void HandleLocalMessage(IMessage message, TaskCompletionSource<CommandResult> taskCompletionSource)
        {
            _logger.DebugFormat("RECV local: {0}", message);

            var context = MessageContext.CreateOverride(PeerId, EndPoint);
            var dispatch = new MessageDispatch(context, message, GetOnLocalMessageDispatchedContinuation(taskCompletionSource))
            {
                IsLocal = true
            };

            _messageDispatcher.Dispatch(dispatch);
        }

        private Action<MessageDispatch, DispatchResult> GetOnLocalMessageDispatchedContinuation(TaskCompletionSource<CommandResult> taskCompletionSource)
        {
            return (dispatch, dispatchResult) =>
            {
                HandleDispatchErrors(dispatch, dispatchResult);

                if (taskCompletionSource == null)
                    return;

                var errorStatus = dispatchResult.Errors.Any() ? CommandResult.GetErrorStatus(dispatchResult.Errors) : dispatch.Context.GetErrorStatus();
                var commandResult = new CommandResult(errorStatus.Code, errorStatus.Message, dispatch.Context.ReplyResponse);
                taskCompletionSource.SetResult(commandResult);
            };
        }

        private void SendTransportMessage(MessageId? messageId, IMessage message, Peer peer, bool logEnabled)
            => SendTransportMessage(messageId, message, new[] { peer }, logEnabled);

        private void SendTransportMessage(MessageId? messageId, IMessage message, IList<Peer> peers, bool logEnabled, bool locallyHandled = false)
        {
            if (peers.Count == 0)
            {
                if (!locallyHandled && logEnabled)
                    _logger.InfoFormat("SEND: {0} with no target peer", message);

                return;
            }

            var transportMessage = ToTransportMessage(message);

            if (messageId != null)
                transportMessage.Id = messageId.Value;

            if (logEnabled)
                LogMessageSend(message, transportMessage, peers);

            SendTransportMessage(transportMessage, peers);
        }

        protected void SendTransportMessage(TransportMessage transportMessage, IList<Peer> peers)
            => _transport.Send(transportMessage, peers, new SendContext());

        private static void LogMessageSend(IMessage message, TransportMessage transportMessage, IList<Peer> peers)
            => _logger.InfoFormat("SEND: {0} to {3} ({2} bytes) [{1}]", message, transportMessage.Id, transportMessage.Content.Length, peers);

        protected void AckTransportMessage(TransportMessage transportMessage)
            => _transport.AckMessage(transportMessage);

        protected TransportMessage ToTransportMessage(IMessage message)
            => _serializer.ToTransportMessage(message, PeerId, EndPoint);

        private IMessage ToMessage(TransportMessage transportMessage)
            => ToMessage(transportMessage.MessageTypeId, transportMessage.Content, transportMessage);

        private IMessage ToMessage(MessageTypeId messageTypeId, Stream messageStream, TransportMessage transportMessage)
        {
            return _serializer.ToMessage(transportMessage, messageTypeId, messageStream);
        }

        public void Dispose()
        {
            if (Status == BusStatus.Started)
                Stop();
        }

   
    }
}
