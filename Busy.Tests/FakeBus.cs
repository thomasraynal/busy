using System;
using System.Threading.Tasks;
using Busy.Infrastructure;

namespace Busy.Tests
{
    internal class FakeBus : IBus
    {
        public PeerId PeerId => throw new NotImplementedException();

        public string Environment => throw new NotImplementedException();

        public bool IsRunning => throw new NotImplementedException();

        public event Action Starting;
        public event Action Started;
        public event Action Stopping;
        public event Action Stopped;

        public void Configure(PeerId peerId, string environment)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Publish(IEvent message)
        {
            throw new NotImplementedException();
        }

        public void Reply(int errorCode)
        {
            throw new NotImplementedException();
        }

        public void Reply(int errorCode, string message)
        {
            throw new NotImplementedException();
        }

        public void Reply(IMessage response)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResult> Send(ICommand message)
        {
            throw new NotImplementedException();
        }

        public Task<CommandResult> Send(ICommand message, Peer peer)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> SubscribeAsync(SubscriptionRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<IDisposable> SubscribeAsync(SubscriptionRequest request, Action<IMessage> handler)
        {
            throw new NotImplementedException();
        }
    }
}