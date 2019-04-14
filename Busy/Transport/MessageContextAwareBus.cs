using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Busy
{
    public class MessageContextAwareBus : IBus
    {
        private readonly IBus _bus;
        public readonly MessageContext MessageContext;

        public MessageContextAwareBus(IBus bus, MessageContext messageContext)
        {
            _bus = bus;
            MessageContext = messageContext;
        }

        public IBus InnerBus => _bus;
        public PeerId PeerId => _bus.PeerId;
        public bool IsRunning => _bus.IsRunning;
        public string DirectoryEndpoint => _bus.DirectoryEndpoint;

        public void Configure(PeerId peerId) => _bus.Configure(peerId);

        public void Publish(IEvent message)
        {
            using (MessageContext.SetCurrent(MessageContext))
            {
                _bus.Publish(message);
            }
        }

        public Task<CommandResult> Send(ICommand message)
        {
            using (MessageContext.SetCurrent(MessageContext))
            {
                return _bus.Send(message);
            }
        }

        public Task<CommandResult> Send(ICommand message, Peer peer)
        {
            using (MessageContext.SetCurrent(MessageContext))
            {
                return _bus.Send(message, peer);
            }
        }

        public Task<IDisposable> SubscribeAsync(SubscriptionRequest request)
            => _bus.SubscribeAsync(request);

        public void Start() => _bus.Start();
        public void Stop() => _bus.Stop();


        public void Dispose() => _bus.Dispose();
    }
}
